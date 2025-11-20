// App.tsx
// -------------
// This is the root React component of the frontend.
// It configures client-side routing and wraps the app in the AuthProvider,
// so all components can access authentication state.

import React from "react";

// BrowserRouter: keeps the UI in sync with the browser URL.
// Routes / Route: define which component to render for each path (URL).
import { BrowserRouter, Routes, Route } from "react-router-dom";

// Layout components
import NavMenu from "./layout/NavMenu";
import HomePage from "./layout/HomePage";
import NotFoundPage from "./layout/NotFoundPage";

// Authentication pages + provider
import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";

// In the future we will also import quiz-related pages here, for example:
// import QuizListPage from "./quiz/QuizListPage";
// import QuizCreatePage from "./quiz/QuizCreatePage";
// import QuizEditPage from "./quiz/QuizEditPage";
// import QuizTakePage from "./quiz/QuizTakePage";
// import QuizResultPage from "./quiz/QuizResultPage";

import QuizListPage from "./quiz/QuizListPage";
import QuizCreatePage from "./quiz/QuizCreatePage";
import QuizTakePage from "./quiz/QuizTakePage";
import QuizEditPage from "./quiz/QuizEditPage";

const App: React.FC = () => {
  return (
    // AuthProvider makes authentication state (user, token, login, logout, etc.)
    // available to all components below it in the component tree.
    <AuthProvider>
      {/* BrowserRouter wraps the entire application and enables routing. */}
      <BrowserRouter>
        {/* The main structure of the app: navigation + page content. */}
        <div className="app-root">
          {/* Top navigation bar, visible on all pages. */}
          <NavMenu />

          {/* Main content area. We use a <main> HTML element for semantics. */}
          <main className="app-main">
            {/* Define all routes here. Only one Route will match at a time. */}
            <Routes>
              {/* Home page: shown at the root URL "/" */}
              <Route path="/" element={<HomePage />} />

              {/* Authentication pages */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              {/* 
                Placeholder routes for future quiz functionality.
                Uncomment these when the corresponding components exist.

                <Route path="/quizzes" element={<QuizListPage />} />
                <Route path="/quizzes/create" element={
                  <ProtectedRoute>
                    <QuizCreatePage />
                  </ProtectedRoute>
                } />
                <Route path="/quizzes/:id/edit" element={
                  <ProtectedRoute>
                    <QuizEditPage />
                  </ProtectedRoute>
                } />
                <Route path="/quizzes/:id/take" element={<QuizTakePage />} />
                <Route path="/results/:id" element={
                  <ProtectedRoute>
                    <QuizResultPage />
                  </ProtectedRoute>
                } />
              */}

              {/* 
                Fallback route: "*" matches any URL that did not match above.
                Used for 404 / Not Found page.
              */}
              <Route path="/quizzes" element={<QuizListPage />} />
              <Route path="/quizzes/create" element={<QuizCreatePage />} />
              <Route path="/quizzes/:id/take" element={<QuizTakePage />} />
              <Route path="/quizzes/:id/edit" element={<QuizEditPage />} />

              
              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </main>
        </div>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default App;
