import { Trash2Icon } from 'lucide-react'
import React from 'react'
import { Trans, useTranslation } from 'react-i18next'

import { useDeleteAccount, useGetAccountById } from '@/api/endpoints/AccountsEndpoints'
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogMedia,
  AlertDialogTitle,
} from '@/components/ui/AlertDialog'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'

import { useAccountsStore } from '../../stores/accountsStore'

function AccountsDeleteFormDialog() {
  const { t } = useTranslation()
  const [confirmation, setConfirmation] = React.useState<string>('')
  const confirmationInputRef = React.useRef<HTMLInputElement>(null)

  const deletingAccountId = useAccountsStore((state) => state.deletingAccountId)
  const { setDeletingAccountId } = useAccountsStore((state) => state.actions)

  const account = useGetAccountById(deletingAccountId)
  const accountName = account?.name ?? ''
  const canDelete = confirmation === accountName

  const deleteAccountMutation = useDeleteAccount()

  const handleOpenChange = React.useCallback(
    (open: boolean) => {
      if (!open) {
        setDeletingAccountId(null)
        setConfirmation('')
      }
    },
    [setDeletingAccountId]
  )

  const handleSubmit = () => {
    if (!deletingAccountId) return

    deleteAccountMutation.mutate(deletingAccountId, {
      onSuccess: () => {
        setDeletingAccountId(null)
        setConfirmation('')
      },
    })
  }

  const isDeleteDisabled = !canDelete || deleteAccountMutation.isPending
  const isDeletePending = deleteAccountMutation.isPending

  return (
    <AlertDialog open={!!deletingAccountId} onOpenChange={handleOpenChange}>
      <AlertDialogContent
        size="default"
        onOpenAutoFocus={(event) => {
          event.preventDefault()
          requestAnimationFrame(() => confirmationInputRef.current?.focus())
        }}
      >
        <AlertDialogHeader>
          <AlertDialogMedia className="bg-destructive/10 text-destructive dark:bg-destructive/20 dark:text-destructive">
            <Trash2Icon />
          </AlertDialogMedia>
          <AlertDialogTitle>{t('accounts.actions.delete.title')}</AlertDialogTitle>
          <AlertDialogDescription>
            <Trans i18nKey="accounts.actions.delete.description" values={{ accountName }} />
            <Input
              ref={confirmationInputRef}
              autoFocus
              className="mt-4"
              value={confirmation}
              onBlur={() => {
                if (!canDelete) {
                  confirmationInputRef.current?.focus()
                }
              }}
              onChange={(e) => setConfirmation(e.target.value)}
              onFocus={() => {
                if (!canDelete) {
                  confirmationInputRef.current?.select()
                }
              }}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  if (canDelete) {
                    handleSubmit()
                  }
                }
              }}
            />
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isDeletePending} variant="outline">
            {t('actions.cancel')}
          </AlertDialogCancel>
          <Button disabled={isDeleteDisabled} loading={isDeletePending} variant="destructive" onClick={handleSubmit}>
            {t('actions.delete')}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}

export { AccountsDeleteFormDialog }
