import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { I18nextProvider } from 'react-i18next'

import { Toaster } from './components/ui/Sonner.tsx'
import { i18next } from './config/I18next.ts'
import './index.css'
import QueryProvider from './providers/QueryProvider.tsx'
import AppRouter from './providers/RouterProvider.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryProvider>
      <I18nextProvider i18n={i18next}>
        <AppRouter />
        <Toaster />
      </I18nextProvider>
    </QueryProvider>
  </StrictMode>
)
