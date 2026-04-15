import { zodResolver } from '@hookform/resolvers/zod'
import React from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { useGetAccountById, usePutAccount } from '@/api/endpoints/AccountsEndpoints'
import { CheckboxControlled } from '@/components/forms/CheckboxControlled'
import { InputControlled } from '@/components/forms/InputControlled'
import { InputNumberControlled } from '@/components/forms/InputNumberControlled'
import { Button } from '@/components/ui/Button'
import { FieldGroup } from '@/components/ui/Field'
import { ResponsiveDialog } from '@/components/ui/ResponsiveDialog'
import { PutAccountFormSchema, type PutAccountSchemaDto, toPutAccountRequest } from '@/types/accounts/forms/PutAccountFormDto'

import { useAccountsStore } from '../../stores/accountsStore'

function AccountsPutFormDialog() {
  const { t } = useTranslation()

  const patchingAccountId = useAccountsStore((state) => state.patchingAccountId)
  const setPatchingAccountId = useAccountsStore((state) => state.actions.setPatchingAccountId)

  const handleOpenChange = (open: boolean) => {
    if (!open) {
      setPatchingAccountId(null)
    }
  }

  return (
    <ResponsiveDialog open={!!patchingAccountId} title={t('accounts.actions.patch.title')} onOpenChange={handleOpenChange}>
      {patchingAccountId && <AccountPutForm accountId={patchingAccountId} />}
    </ResponsiveDialog>
  )
}

function AccountPutForm({ accountId }: { accountId: string }) {
  const { t } = useTranslation()
  const setPatchingAccountId = useAccountsStore((state) => state.actions.setPatchingAccountId)

  const account = useGetAccountById(accountId)
  const putAccountMutation = usePutAccount()

  const form = useForm<PutAccountSchemaDto>({
    resolver: zodResolver(PutAccountFormSchema),
    defaultValues: {
      name: account?.name ?? '',
      bank: account?.bank ?? '',
      updateInitialBalance: false,
      initialBalance: account?.initialBalance ?? undefined,
    },
  })

  const handleSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    form.handleSubmit((data) => {
      putAccountMutation.mutate(
        { id: accountId, data: toPutAccountRequest(data) },
        {
          onSuccess: () => {
            setPatchingAccountId(null)
          },
        }
      )
    })(event)
  }

  const isPatchInitialBalance = useWatch({ control: form.control, name: 'updateInitialBalance' })
  const isSubmitDisabled = putAccountMutation.isSuccess || putAccountMutation.isPending
  const isSubmitPending = putAccountMutation.isPending

  return (
    <form className="grid items-start gap-6 p-4" onSubmit={handleSubmit}>
      <FieldGroup>
        <InputControlled autoFocus control={form.control} disabled={isSubmitPending} label={t('accounts.name')} name="name" />
        <InputControlled control={form.control} disabled={isSubmitPending} label={t('accounts.bank')} name="bank" />
        <CheckboxControlled
          control={form.control}
          disabled={isSubmitPending}
          label={t('accounts.actions.patch.updateInitialBalance')}
          name="updateInitialBalance"
        />
        {isPatchInitialBalance && (
          <InputNumberControlled
            fixedDecimalScale
            control={form.control}
            decimalScale={2}
            disabled={isSubmitPending}
            label={t('accounts.initialBalance')}
            name="initialBalance"
            suffix=" €"
            thousandSeparator=" "
          />
        )}
      </FieldGroup>
      <Button disabled={isSubmitDisabled} loading={isSubmitPending} type="submit">
        {t('actions.save')}
      </Button>
    </form>
  )
}

export { AccountsPutFormDialog }
