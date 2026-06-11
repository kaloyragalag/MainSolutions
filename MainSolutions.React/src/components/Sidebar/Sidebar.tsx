import React, { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import './Sidebar.css';

interface NavItem {
  label: string;
  path?: string;
  icon: React.ReactNode;
  children?: { label: string; path: string }[];
}

const HomeIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/>
    <polyline points="9 22 9 12 15 12 15 22"/>
  </svg>
);

const ConfigIcon = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <circle cx="12" cy="12" r="3"/>
    <path d="M19.07 4.93a10 10 0 010 14.14M4.93 19.07a10 10 0 010-14.14M12 2v2M12 20v2M2 12h2M20 12h2"/>
  </svg>
);

const CategoryIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/>
    <rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/>
  </svg>
);

const ProductIcon = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
    <path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z"/>
    <line x1="3" y1="6" x2="21" y2="6"/>
    <path d="M16 10a4 4 0 01-8 0"/>
  </svg>
);

const ChevronIcon = ({ open }: { open: boolean }) => (
  <svg
    width="12" height="12" viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="2" aria-hidden="true"
    style={{ transform: open ? 'rotate(90deg)' : 'rotate(0deg)', transition: 'transform 0.2s ease' }}
  >
    <polyline points="9 18 15 12 9 6"/>
  </svg>
);

const navItems: NavItem[] = [
  {
    label: 'Home',
    path: '/',
    icon: <HomeIcon />,
  },
  {
    label: 'Configuration',
    icon: <ConfigIcon />,
    children: [
      { label: 'Categories', path: '/configuration/categories' },
      { label: 'Products', path: '/configuration/products' },
    ],
  },
];

const Sidebar: React.FC = () => {
  const location = useLocation();
  const [openGroups, setOpenGroups] = useState<string[]>(['Configuration']);

  const toggleGroup = (label: string) => {
    setOpenGroups(prev =>
      prev.includes(label) ? prev.filter(l => l !== label) : [...prev, label]
    );
  };

  const isGroupActive = (item: NavItem) =>
    item.children?.some(child => location.pathname.startsWith(child.path)) ?? false;

  return (
    <aside className="sidebar">
      <nav className="sidebar__nav" aria-label="Main navigation">
        {navItems.map(item => (
          <div key={item.label}>
            {item.path ? (
              <NavLink
                to={item.path}
                end
                className={({ isActive }) =>
                  `sidebar__item ${isActive ? 'sidebar__item--active' : ''}`
                }
              >
                <span className="sidebar__item-icon">{item.icon}</span>
                <span className="sidebar__item-label">{item.label}</span>
              </NavLink>
            ) : (
              <>
                <button
                  className={`sidebar__item sidebar__item--group ${isGroupActive(item) ? 'sidebar__item--group-active' : ''}`}
                  onClick={() => toggleGroup(item.label)}
                  aria-expanded={openGroups.includes(item.label)}
                >
                  <span className="sidebar__item-icon">{item.icon}</span>
                  <span className="sidebar__item-label">{item.label}</span>
                  <span className="sidebar__item-chevron">
                    <ChevronIcon open={openGroups.includes(item.label)} />
                  </span>
                </button>

                {openGroups.includes(item.label) && item.children && (
                  <div className="sidebar__children">
                    {item.children.map(child => (
                      <NavLink
                        key={child.path}
                        to={child.path}
                        className={({ isActive }) =>
                          `sidebar__child ${isActive ? 'sidebar__child--active' : ''}`
                        }
                      >
                        <span className="sidebar__child-icon">{child.label === 'Categories' ? <CategoryIcon /> : <ProductIcon />}</span>
                        {child.label}
                      </NavLink>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
        ))}
      </nav>
    </aside>
  );
};

export default Sidebar;
