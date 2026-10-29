import { createContext } from 'react'
import type { ReactNode } from 'react'
import { useCallback, useMemo, useState } from 'react'
import type { AuthState, LoginCredentials, User } from '../types/auth'
import { authService } from '../services/authService'

const TOKEN_KEY = 'budgetflow_token'

interface AuthContextValue extends AuthState {
  login: (credentials: LoginCredentials) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY))
  const [user, setUser] = useState<User | null>(null)

  const login = useCallback(async (credentials: LoginCredentials) => {
    const response = await authService.login(credentials)
    localStorage.setItem(TOKEN_KEY, response.accessToken)
    setToken(response.accessToken)
    const currentUser = await authService.getCurrentUser()
    setUser(currentUser)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    setToken(null)
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token),
      login,
      logout,
    }),
    [user, token, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
