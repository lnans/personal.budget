import { zodResolver } from '@hookform/resolvers/zod'
import React from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { useGetAccountById, usePatchAccount } from '@/api/endpoints/AccountsEndpoints'
import { CheckboxControlled } from '@/components/forms/CheckboxControlled'
import { InputControlled } from '@/components/forms/InputControlled'
import { InputNumberControlled } from '@/components/forms/InputNumberControlled'
import { Button } from '@/components/ui/Button'
import { FieldGroup } from '@/components/ui/Field'
import { ResponsiveDialog } from '@/components/ui/ResponsiveDialog'
import {
  PatchAccountFormSchema,
  toPatchAccountRequest,
  type PatchAccountSchemaDto,
} from '@/types/accounts/forms/PatchAccountFormDto'

import { useAccountsStore } from '../../stores/accountsStore'

function AccountsPatchFormDialog() {
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
      {patchingAccountId && <AccountPatchForm accountId={patchingAccountId} />}
    </ResponsiveDialog>
  )
}

function AccountPatchForm({ accountId }: { accountId: string }) {
  const { t } = useTranslation()
  const setPatchingAccountId = useAccountsStore((state) => state.actions.setPatchingAccountId)

  const account = useGetAccountById(accountId)
  const patchAccountMutation = usePatchAccount()

  const form = useForm<PatchAccountSchemaDto>({
    resolver: zodResolver(PatchAccountFormSchema),
    defaultValues: {
      name: account?.name ?? '',
      bank: account?.bank ?? '',
      updateInitialBalance: false,
      initialBalance: account?.initialBalance ?? undefined,
    },
  })

  const handleSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
    form.handleSubmit((data: PatchAccountSchemaDto) => {
      patchAccountMutation.mutate(
        { id: accountId, data: toPatchAccountRequest(data) },
        {
          onSuccess: () => {
            setPatchingAccountId(null)
          },
        }
      )
    })(event)
  }

  const isPatchInitialBalance = useWatch({ control: form.control, name: 'updateInitialBalance' })
  const isSubmitDisabled = patchAccountMutation.isSuccess || patchAccountMutation.isPending
  const isSubmitPending = patchAccountMutation.isPending

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

export { AccountsPatchFormDialog }
