import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { useAuthStore } from '@/modules/auth/authStore'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'

describe('ProtectedRoute', () => {
  beforeEach(() => {
    useAuthStore.getState().clearSession()
  })

  afterEach(() => {
    cleanup()
  })

  it('redirects private routes to login when there is no session', async () => {
    render(
      <MemoryRouter initialEntries={['/settings']}>
        <Routes>
          <Route
            element={
              <ProtectedRoute>
                <div>Private settings</div>
              </ProtectedRoute>
            }
            path="/settings"
          />
          <Route element={<div>Login screen</div>} path="/login" />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Login screen')).toBeTruthy()
    expect(screen.queryByText('Private settings')).toBeNull()
  })

  it('clears expired sessions before rendering private routes', async () => {
    useAuthStore.getState().setSession({
      accessToken: 'expired-access-token',
      expiresAt: '2026-01-01T00:00:00.000Z',
      refreshToken: 'expired-refresh-token',
      user: {
        branchId: '22222222-2222-2222-2222-222222222222',
        businessId: '11111111-1111-1111-1111-111111111111',
        email: 'admin@test.com',
        fullName: 'Admin',
        id: '33333333-3333-3333-3333-333333333333',
        roles: ['Admin'],
      },
    })

    render(
      <MemoryRouter initialEntries={['/products']}>
        <Routes>
          <Route
            element={
              <ProtectedRoute>
                <div>Private products</div>
              </ProtectedRoute>
            }
            path="/products"
          />
          <Route element={<div>Login screen</div>} path="/login" />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Login screen')).toBeTruthy()
    expect(useAuthStore.getState().session).toBeNull()
  })
})
