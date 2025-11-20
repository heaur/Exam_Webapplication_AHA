using AutoMapper;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
using QuizApi.Application.Mapping;
using QuizApi.Controllers; 
using QuizApi.DAL; 
using QuizApi.Domain; 
using QuizApi.DTOs; 
using Xunit;



public class QuizControllerTests
{
    //lager en InMemory database for hver test 
    //fra forelesning om EF core og unit testing 
    //hver test får sin "egen" database
    //ingen tester påvirker hverandre 
    private QuizDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuizDbContext(options);
    }
    
    //hjelpemetode for å lage en Quizcontroller med automapper, fake httpcontext, testene ligner derfor reele kal i web API

    private QuizController GetController(QuizDbContext db, string userId = "user1")
    {
        //Automapp med samme profil som i koden 
        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        }).CreateMapper();

        var controller = new QuizController(db, mapper);

        // fake logged-in user (from SecurityTesting)
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) },
            "TestAuth"
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }


    // positive test: bruker sender inn gyldig QuizCreateDto, 
    // Forventninger: Controlleren returnerer 201 created, et quiz objekt blir lagret i datatbase.
    [Fact]
    public async Task CreateQuiz_ReturnsCreated_WhenValid()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);


        //gyldig input 
        var dto = new QuizCreateDto("My Quiz", "Test");

        var result = await controller.Create(dto);

        //sjekker HTTP respons type
        Assert.IsType<CreatedAtActionResult>(result.Result);
        //sjekker at det faktisk ble oprettet ett quiz objekt i databasen 
        Assert.Equal(1, db.Quizzes.Count());
    }

    // 2. CREATE NEGATIVE, scenario: bruker sender inn ugyldig data når de skal opprette test, mangler tittel på quiz
    //simulerer feil gjennom ModelState
    //forventing: controlleren sender ut 400 badrequest 
    [Fact]
    public async Task CreateQuiz_ReturnsBadRequest_WhenInvalid()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);

        //ugyldig input mangler titel
        var dto = new QuizCreateDto("", "desc");
        //simulerer at model validering har feilet (runtime)
        controller.ModelState.AddModelError("Title", "Required");

        var result = await controller.Create(dto);

        //sjekker at API svarer med bad request ved en valideringsfeil 
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // 3. READ POSITIVE
    //scenario: det finnes en quiz med ID=1 i databasen 
    //forventinger: GetByID(1) returnerer 200 ok med data (okobjectresult)
    [Fact]
    public async Task GetQuiz_ReturnsOk_WhenExists()

    {   //testdata legges inn i minnedatabasen
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Demo", OwnerId = "user1" });
        db.SaveChanges();

        var controller = GetController(db);

        var result = await controller.GetById(1);

        //forventet respons 200 OK
        Assert.IsType<OkObjectResult>(result.Result);
    }

    // 4. READ NEGATIVE

    //scenario: det finnes ikke quiz med id=999
    //forventninger: getbyid returnerer 404 notfound 
    [Fact]
    public async Task GetQuiz_ReturnsNotFound_WhenMissing()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);

        var result = await controller.GetById(999);

        //forventet respons 404 not found siden ressursen ikke finnes 
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // 5. UPDATE POSITIVE
    //scenario: en quiz eksisterer i databasen eid av user 1
    //innlogget bruker i testes er user 1
    //bruker sender inn gyldig QuizUpdatdeDTO
    //Forventning: Controlleren returnerer 404 noconnection 
    //tittelen i databsen blir oppdatert
    [Fact]
    public async Task UpdateQuiz_ReturnsNoContent_WhenOwner()
    {
        //quiz laget av user 1 (eksisterende)
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Old", OwnerId = "user1" });
        db.SaveChanges();
        //simulerer at bruker som er logget inn er user 1
        var controller = GetController(db, "user1");

        var dto = new QuizUpdateDto("New Title", "New Desc", null);

        var result = await controller.Update(1, dto);

        //forventer at 204 noconection ved vellykket oppdatering 
        Assert.IsType<NoContentResult>(result);
        //sjekker at tittelen i databasen faktisk ble endret 
        Assert.Equal("New Title", db.Quizzes.First().Title);
    }

    // 6. UPDATE NEGATIVE (not owner)
    //scenario: en quiz eies av owner 1
    //innlogget bruker i testen er otheruser 
    //forventning: update skal ikke være lov + frobineresult (403)
    [Fact]
    public async Task UpdateQuiz_ReturnsForbid_WhenNotOwner()
    {
        var db = GetInMemoryDb();
        //quiz tilhører en annen bruker
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Old", OwnerId = "owner1" });
        db.SaveChanges();

        //simulerer bruker som ikke er eier 
        var controller = GetController(db, "otherUser");

        var dto = new QuizUpdateDto("Hack", "Hack", null);

        var result = await controller.Update(1, dto);

        //forventet respons: forbiden (403) når ikke eier prøver å oppdatere 
        Assert.IsType<ForbidResult>(result);
    }

    // 7. DELETE POSITIVE
    //scenario en quiz eier av user 1
    //innlogget bruker er user1
    //Forventninger delete returnerer 204 no conect, quiz blir fjernet fra databasen 
    [Fact]
    public async Task DeleteQuiz_ReturnsNoContent_WhenOwner()
    {
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Delete", OwnerId = "user1" });
        db.SaveChanges();

        var controller = GetController(db);

        var result = await controller.Delete(1);

        //forventet resultat 204 nocontent
        Assert.IsType<NoContentResult>(result);
        //sjekker at det ikke finnes noen quiz
        Assert.Equal(0, db.Quizzes.Count());
    }

    // 8. DELETE NEGATIVE
    //Scenario: det finnes ikke en quiz med id 999
    //forventninger: delete 999 returernerer 404 not found 
    [Fact]
    public async Task DeleteQuiz_ReturnsNotFound_WhenMissing()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);

        var result = await controller.Delete(999);


        //forventet respons er 404 not found da man prøver å slette noe som ikke finnes 
        Assert.IsType<NotFoundResult>(result);
    }
}
