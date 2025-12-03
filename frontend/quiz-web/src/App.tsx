// src/App.tsx
// -----------
// Root component that sets up routing and wraps the app in AuthProvider.

import React from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import NavMenu from "./layout/NavMenu";
import HomePage from "./layout/HomePage";
import NotFoundPage from "./layout/NotFoundPage";

import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";

import QuizCreatePage from "./quiz/QuizCreatePage";
import QuizTakePage from "./quiz/QuizTakePage";
import QuizResultPage from "./quiz/QuizResultPage";
import QuizEditPage from "./quiz/QuizEditPage";

import ProfilePage from "./profile/ProfilePage";

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        {/* app-root + app-main matcher index.css-layouten */}
        <div className="app-root">
          <NavMenu />
          <main className="app-main">
            <Routes>
              <Route path="/" element={<HomePage />} />

              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              <Route path="/quizzes/create" element={<QuizCreatePage />} />
              <Route path="/quizzes/:id/take" element={<QuizTakePage />} />
            
              <Route path="/quizzes/:id/result" element={<QuizResultPage />} />
              
              <Route path="/quizzes/:id/edit" element={<QuizEditPage />} />

              <Route path="/profile" element={<ProfilePage />} />

              <Route path="*" element={<NotFoundPage />} />
            </Routes>
          </main>
        </div>
      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;
