import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import { setTokens, clearTokens, getTokens } from '../utils/token';

export interface User {
  Id: string;
  Username: string;
  Email: string;
  Role: string;
}

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: any) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const parseJwt = (token: string) => {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
};

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    const initAuth = () => {
      const { accessToken } = getTokens();
      if (accessToken) {
        const decoded = parseJwt(accessToken);
        if (decoded && decoded.exp * 1000 > Date.now()) {
          setUser({
            Id: decoded.nameid || decoded.sub || '',
            Username: decoded.unique_name || decoded.name || '',
            Email: decoded.email || '',
            Role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || 'USER',
          });
        } else {
          clearTokens();
        }
      }
      setIsLoading(false);
    };
    initAuth();
  }, []);

  const login = async (credentials: any) => {
    const response = await api.post('/auth/login', credentials);
    const { accessToken, refreshToken } = response.data.data;
    
    setTokens(accessToken, refreshToken);
    
    const decoded = parseJwt(accessToken);
    setUser({
      Id: decoded.nameid || decoded.sub || '',
      Username: decoded.unique_name || decoded.name || '',
      Email: decoded.email || '',
      Role: decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role || 'USER',
    });
    
    navigate('/');
  };

  const logout = () => {
    clearTokens();
    setUser(null);
    navigate('/login', { replace: true });
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout }}>
      {!isLoading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};