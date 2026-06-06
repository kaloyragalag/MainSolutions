import React from 'react';
import Navbar from './Navbar';
import './BasePage.css';

interface BasePageProps {
  children: React.ReactNode;
}

const BasePage: React.FC<BasePageProps> = ({ children }) => {
  return (
    <div className="base-page">
      <Navbar />
      <main className="base-page__main">
        {children}
      </main>
    </div>
  );
};

export default BasePage;
