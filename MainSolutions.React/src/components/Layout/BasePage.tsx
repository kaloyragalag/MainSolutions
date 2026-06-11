import React from 'react';
import Navbar from './Navbar';
import Sidebar from '../Sidebar/Sidebar';
import './BasePage.css';

interface BasePageProps {
  children: React.ReactNode;
  showSidebar?: boolean;
}

const BasePage: React.FC<BasePageProps> = ({ children, showSidebar = true }) => {
  return (
    <div className="base-layout">
      <Navbar />
      {showSidebar && <Sidebar />}
      <main className={`base-layout__main${showSidebar ? '' : ' base-layout__main--no-sidebar'}`}>
        {children}
      </main>
    </div>
  );
};

export default BasePage;
