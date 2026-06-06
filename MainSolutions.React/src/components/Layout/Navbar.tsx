import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Navbar.css';

const Navbar: React.FC = () => {
  const { user, logout, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = useState(false);

  const handleLogout = () => {
    logout();
    setDropdownOpen(false);
    navigate('/');
  };

  const initials = user
    ? `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
    : '';

  return (
    <nav className="navbar">
      <div className="navbar__inner">
        <Link to="/" className="navbar__brand">
          <span className="navbar__brand-bracket">{`{`}</span>
          <span className="navbar__brand-text">MS</span>
          <span className="navbar__brand-bracket">{`}`}</span>
        </Link>

        <div className="navbar__actions">
          {isAuthenticated && user ? (
            <div className="navbar__user">
              <button
                className="navbar__avatar"
                onClick={() => setDropdownOpen(prev => !prev)}
                aria-label="User menu"
                aria-expanded={dropdownOpen}
              >
                <span className="navbar__initials">{initials}</span>
              </button>
              <span className="navbar__username">
                {user.firstName} {user.lastName}
              </span>

              {dropdownOpen && (
                <div className="navbar__dropdown">
                  <div className="navbar__dropdown-header">
                    <p className="navbar__dropdown-name">{user.firstName} {user.lastName}</p>
                    <p className="navbar__dropdown-email">{user.email}</p>
                  </div>
                  <button className="navbar__dropdown-item navbar__dropdown-item--danger" onClick={handleLogout}>
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                      <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4" />
                      <polyline points="16 17 21 12 16 7" />
                      <line x1="21" y1="12" x2="9" y2="12" />
                    </svg>
                    Sign out
                  </button>
                </div>
              )}
            </div>
          ) : (
            <>
              <Link to="/login" className="navbar__link">Sign in</Link>
              <Link to="/register" className="navbar__btn">Register</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
