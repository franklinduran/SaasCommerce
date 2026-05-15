import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { LoginForm } from '@/modules/auth/components/LoginForm'

describe('LoginForm', () => {
  it('shows validation errors for empty credentials', async () => {
    const user = userEvent.setup()

    renderLoginForm()

    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByText('Correo invalido')).toBeTruthy()
    expect(
      await screen.findByText('La contrasena debe tener al menos 6 caracteres'),
    ).toBeTruthy()
  })
})

function renderLoginForm() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <LoginForm />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}
