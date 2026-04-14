import { GalleryVerticalEnd } from 'lucide-react'
import { useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { SignInForm } from '@/features/authentication/components/SignInForm'
import { useAuthStore } from '@/features/authentication/stores/authStore'

function AuthPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const { authToken } = useAuthStore((state) => state)
  const { isAuthTokenValid } = useAuthStore((state) => state.actions)

  useEffect(() => {
    if (authToken && isAuthTokenValid()) {
      navigate('/', { replace: true })
    }
  }, [authToken, isAuthTokenValid, navigate])

  if (authToken && isAuthTokenValid()) {
    return null
  }

  return (
    <div className="bg-muted flex min-h-svh flex-col items-center justify-center gap-6 p-6 md:p-10">
      <div className="flex w-full max-w-sm flex-col gap-6">
        <a className="flex items-center gap-2 self-center font-medium text-2xl" href="#">
          <div className="bg-primary text-primary-foreground flex size-6 items-center justify-center rounded-md">
            <GalleryVerticalEnd className="size-4" />
          </div>
          {t('app.brand')}
        </a>
        <SignInForm />
      </div>
    </div>
  )
}

export { AuthPage }
