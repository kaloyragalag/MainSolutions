import React from 'react';
import BasePage from '../../components/Layout/BasePage';
import { useAuth } from '../../context/AuthContext';
import './HomePage.css';

const HomePage: React.FC = () => {
  const { user } = useAuth();

  return (
    <BasePage>
      <div className="ms-page-header">
        <div className="ms-page-header__info">
          <h1 className="ms-page-header__title">Welcome back, {user?.firstName}</h1>
          <p className="ms-page-header__subtitle">Here's what's happening in your workspace today.</p>
        </div>
      </div>
    </BasePage>
  );
};

export default HomePage;
