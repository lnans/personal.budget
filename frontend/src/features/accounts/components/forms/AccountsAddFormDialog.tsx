import { zodResolver } from '@hookform/resolvers/zod'
import React from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { useCreateAccount } from '@/api/endpoints/AccountsEndpoints'
import { InputControlled } from '@/components/forms/InputControlled'
import { InputNumberControlled } from '@/components/forms/InputNumberControlled'
import { SelectControlled } from '@/components/forms/SelectControlled'
import { Button } from '@/components/ui/Button'
import { FieldGroup } from '@/components/ui/Field'
import { ResponsiveDialog } from '@/components/ui/ResponsiveDialog'
import { CreateAccountFormSchema, type CreateAccountFormDto } from '@/types/accounts/forms/CreateAccountFormDto'

import { useAccountsStore } from '../../stores/accountsStore'

function AccountsAddFormDialog() {
  const { t } = useTranslation()

  const isCreatingAccount = useAccountsStore((state) => state.isCreatingAccount)
  const setIsCreatingAccount = useAccountsStore((state) => state.actions.setIsCreatingAccount)

  return (
    <ResponsiveDialog open={isCreatingAccount} title={t('accounts.actions.add.title')} onOpenChange={setIsCreatingAccount}>
      <AccountAddForm />
    </ResponsiveDialog>
  )
}

function AccountAddForm() {
  const { t } = useTranslation()
  const setIsCreatingAccount = useAccountsStore((state) => state.actions.setIsCreatingAccount)

  const createAccountMutation = useCreateAccount()

  const form = useForm<CreateAccountFormDto>({
    resolver: zodResolver(CreateAccountFormSchema),
    defaultValues: {
      name: '',
      bank: '',
      type: 'Checking',
      initialBalance: 0,
    },
  })

  const handleSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    form.handleSubmit((data: CreateAccountFormDto) => {
      createAccountMutation.mutate(data, {
        onSuccess: () => {
          setIsCreatingAccount(false)
        },
      })
    })(event)
  }

  const accountTypes = [
    { value: 'Checking', label: t('accounts.type.Checking') },
    { value: 'Savings', label: t('accounts.type.Savings') },
  ]

  const isSubmitDisabled = createAccountMutation.isSuccess || createAccountMutation.isPending
  const isSubmitPending = createAccountMutation.isPending

  return (
    <form className="grid items-start gap-6 p-4" onSubmit={handleSubmit}>
      <FieldGroup>
        <InputControlled autoFocus control={form.control} disabled={isSubmitPending} label={t('accounts.name')} name="name" />
        <InputControlled control={form.control} disabled={isSubmitPending} label={t('accounts.bank')} name="bank" />
        <div className="grid grid-cols-2 gap-2">
          <SelectControlled
            control={form.control}
            disabled={isSubmitPending}
            label={t('accounts.type')}
            name="type"
            options={accountTypes}
          />
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
        </div>
      </FieldGroup>
      <Button disabled={isSubmitDisabled} loading={isSubmitPending} type="submit">
        {t('actions.save')}
      </Button>
    </form>
  )
}

export { AccountsAddFormDialog }
