import React from 'react';
import './App.css';

const App: React.FC = () => {
  return (
    <div className="home">
      <div className="home__content">
        <div className="home__logo">
          <span className="home__logo-bracket">{`{`}</span>
          <span className="home__logo-text">MS</span>
          <span className="home__logo-bracket">{`}`}</span>
        </div>
        <h1 className="home__title">MainSolutions</h1>
        <p className="home__subtitle">Your application is ready to build.</p>
        <div className="home__actions">
          <a
            className="home__btn home__btn--primary"
            href="https://localhost:5001/swagger"
            target="_blank"
            rel="noopener noreferrer"
          >
            Open API Docs
          </a>
          <a
            className="home__btn home__btn--secondary"
            href="https://react.dev"
            target="_blank"
            rel="noopener noreferrer"
          >
            React Docs
          </a>
        </div>
      </div>
    </div>
  );
};

export default App;
