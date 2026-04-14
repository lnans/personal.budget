import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom'

import App from '@/App'
import { AppLoader } from '@/components/ui/AppLoader'
import { lazyImport } from '@/lib/lazyimport'
import AuthProvider from '@/providers/AuthProvider'

const { AuthPage } = lazyImport(() => import('@/app/auth/AuthPage'), 'AuthPage', <AppLoader />)
const { MainLayout } = lazyImport(() => import('@/app/main/MainLayout'), 'MainLayout', <AppLoader />)
const { OperationsPage } = lazyImport(() => import('@/app/main/operations/OperationsPage'), 'OperationsPage', <AppLoader />)

function ProtectedLayout() {
  return (
    <AuthProvider>
      <App />
    </AuthProvider>
  )
}

function AppRouter() {
  const router = createBrowserRouter([
    {
      path: '/auth',
      element: <AuthPage />,
    },
    {
      path: '/',
      element: <ProtectedLayout />,
      children: [
        {
          path: '/',
          element: <MainLayout />,
          children: [
            {
              path: '/',
              element: <Navigate replace to="/operations" />,
            },
            {
              path: '/operations',
              element: <OperationsPage />,
            },
          ],
        },
      ],
    },
  ])
  return <RouterProvider router={router} />
}

export default AppRouter
