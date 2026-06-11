import React from 'react';
import Navbar from './Navbar';
import Sidebar from '../Sidebar/Sidebar';
import './BasePage.css';

interface BasePageProps {
  children: React.ReactNode;
}

const BasePage: React.FC<BasePageProps> = ({ children }) => {
  return (
    <div className="base-layout">
      <Navbar />
      <Sidebar />
      <main className="base-layout__main">
        {children}
      </main>
    </div>
  );
};

export default BasePage;
