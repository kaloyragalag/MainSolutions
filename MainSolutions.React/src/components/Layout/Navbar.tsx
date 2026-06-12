import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import './Navbar.css';

const Navbar: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = React.useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const initials = user?.username?.length
    ? user.username[0].toUpperCase()
    : '';

  return (
    <header className="topbar">
      <div className="topbar__left">
        <Link to="/" className="topbar__brand">
          <div className="topbar__logo">
            <span className="topbar__logo-bracket">{`{`}</span>
            <span className="topbar__logo-text">MS</span>
            <span className="topbar__logo-bracket">{`}`}</span>
          </div>
          <span className="topbar__app-name">MainSolutions</span>
        </Link>
      </div>

      <div className="topbar__right">
        {user && (
          <div className="topbar__user">
            <button
              className="topbar__avatar"
              onClick={() => setDropdownOpen(p => !p)}
              aria-label="User menu"
              aria-expanded={dropdownOpen}
            >
              {initials}
            </button>

            {dropdownOpen && (
              <div className="topbar__dropdown">
                <div className="topbar__dropdown-header">
                  <p className="topbar__dropdown-name">{user.username}</p>
                  <p className="topbar__dropdown-email">{user.email}</p>
                </div>
                <button className="topbar__dropdown-item topbar__dropdown-item--danger" onClick={handleLogout}>
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                    <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4"/>
                    <polyline points="16 17 21 12 16 7"/>
                    <line x1="21" y1="12" x2="9" y2="12"/>
                  </svg>
                  Sign out
                </button>
              </div>
            )}
          </div>
        )}
      </div>
    </header>
  );
};

export default Navbar;
