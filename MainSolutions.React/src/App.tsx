import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import BasePage from './components/Layout/BasePage';
import LoginPage from './pages/Login/LoginPage';
import RegisterPage from './pages/Register/RegisterPage';
import './App.css';

const Home: React.FC = () => (
  <BasePage>
    <div className="home">
      <div className="home__content">
        <div className="home__logo">
          <span className="home__logo-bracket">{`{`}</span>
          <span className="home__logo-text">MS</span>
          <span className="home__logo-bracket">{`}`}</span>
        </div>
        <h1 className="home__title">MainSolutions</h1>
        <p className="home__subtitle">Your application is ready to build.</p>
      </div>
    </div>
  </BasePage>
);

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
};

export default App;
