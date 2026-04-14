import type { ComponentProps } from 'react'
import { Controller, type FieldPath, type FieldValues, type UseControllerProps } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { Field, FieldDescription, FieldError, FieldLabel } from '@/components/ui/Field'
import { Input } from '@/components/ui/Input'

type InputTextControlledProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = ComponentProps<typeof Input> &
  UseControllerProps<TFieldValues, TName> & {
    label: string
    description?: string
    placeholder?: string
  }

function InputControlled<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>({ control, name, label, description, placeholder, defaultValue, ...props }: InputTextControlledProps<TFieldValues, TName>) {
  const { t } = useTranslation()
  return (
    <Controller
      control={control}
      defaultValue={defaultValue}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <Input
            {...props}
            {...field}
            aria-invalid={fieldState.invalid}
            autoComplete="off"
            id={field.name}
            placeholder={placeholder}
          />
          <FieldDescription>{description}</FieldDescription>
          <FieldError errors={fieldState.error?.message ? [{ message: t(fieldState.error.message) }] : undefined} />
        </Field>
      )}
    />
  )
}

export { InputControlled }
