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
    //create an InMemory database for each test
//from a lecture on EF Core and unit testing
//each test gets its "own" database
//no tests affect each other
    private QuizDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<QuizDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QuizDbContext(options);
    }
    
    
    //helper method to create a QuizController with automapper,
// fake HttpContext, so the tests therefore resemble real calls in the web API

    private QuizController GetController(QuizDbContext db, string userId = "user1")
    {
      //Automap with the same profile as in the code
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


   // positive test: user submits a valid QuizCreateDto,
// Expectations: The controller returns 201 created,
// a quiz object is saved in the database.
    [Fact]
    public async Task CreateQuiz_ReturnsCreated_WhenValid()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);


        //valid input 
        var dto = new QuizCreateDto("My Quiz", "Test");

        var result = await controller.Create(dto);

        //checking HTTP respons type
        Assert.IsType<CreatedAtActionResult>(result.Result);
        //checking that a quiz object was actually created in the database
        Assert.Equal(1, db.Quizzes.Count());
    }

   // 2. CREATE NEGATIVE, scenario: user submits invalid data when trying to create a test, missing quiz title
//simulate error through ModelState
//expectation: the controller returns 400 bad request
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
//scenario: there is a quiz with ID=1 in the database
//expectations: GetByID(1) returns 200 ok with data (okobjectresult)
    [Fact]
    public async Task GetQuiz_ReturnsOk_WhenExists()

    {   //test data is entered into the memory database
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Demo", OwnerId = "user1" });
        db.SaveChanges();

        var controller = GetController(db);

        var result = await controller.GetById(1);

       //expected response 200 OK
        Assert.IsType<OkObjectResult>(result.Result);
    }

    // 4. READ NEGATIVE

//scenario: there is no quiz with id=999
//expectations: getbyid returns 404 not found 
    [Fact]
    public async Task GetQuiz_ReturnsNotFound_WhenMissing()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);

        var result = await controller.GetById(999);

        //expected response 404 not found since the resource does not exist
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // 5. UPDATE POSITIVE
//scenario: a quiz exists in the database owned by user 1
//the logged-in user being tested is user 1
//user submits a valid QuizUpdateDTO
//Expectation: The controller returns 404 noconnection
//the title in the database is updated
    [Fact]
    public async Task UpdateQuiz_ReturnsNoContent_WhenOwner()
    {
        //quiz created by user 1 (existing)
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Old", OwnerId = "user1" });
        db.SaveChanges();
        //simulates that a logged-in user is user 1
        var controller = GetController(db, "user1");

        var dto = new QuizUpdateDto("New Title", "New Desc", null);

        var result = await controller.Update(1, dto);

        //expecting 204 no connection on successful update
        Assert.IsType<NoContentResult>(result);
       //checks that the title in the database was actually changed
        Assert.Equal("New Title", db.Quizzes.First().Title);
    }

   // 6. UPDATE NEGATIVE (not owner)
//scenario: a quiz is owned by owner 1
//logged in user in the test is otheruser
//expectation: update should not be allowed + forbidden result (403)
    [Fact]
    public async Task UpdateQuiz_ReturnsForbid_WhenNotOwner()
    {
        var db = GetInMemoryDb();
        //quiz belongs to another user
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Old", OwnerId = "owner1" });
        db.SaveChanges();
//simulates user who is not the owner
        var controller = GetController(db, "otherUser");

        var dto = new QuizUpdateDto("Hack", "Hack", null);

        var result = await controller.Update(1, dto);

       //expected response: forbidden (403) when someone other than the owner tries to update 
        Assert.IsType<ForbidResult>(result);
    }

   // 7. DELETE POSITIVE
//scenario a quiz owned by user 1
//logged-in user is user1
//Expectations delete returns 204 no content, quiz is removed from the database
    [Fact]
    public async Task DeleteQuiz_ReturnsNoContent_WhenOwner()
    {
        var db = GetInMemoryDb();
        db.Quizzes.Add(new Quiz { QuizId = 1, Title = "Delete", OwnerId = "user1" });
        db.SaveChanges();

        var controller = GetController(db);

        var result = await controller.Delete(1);

        //expected result 204 no content
        Assert.IsType<NoContentResult>(result);
        //checking that there is no quiz
        Assert.Equal(0, db.Quizzes.Count());
    }

    // 8. DELETE NEGATIVE
//Scenario: there is no quiz with id 999
//expectations: deleting 999 returns 404 not found 
    [Fact]
    public async Task DeleteQuiz_ReturnsNotFound_WhenMissing()
    {
        var db = GetInMemoryDb();
        var controller = GetController(db);

        var result = await controller.Delete(999);


        //expected response is 404 not found when trying to delete something that does not exist
        Assert.IsType<NotFoundResult>(result);
    }
}
