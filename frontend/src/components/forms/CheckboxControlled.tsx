import type { ComponentProps } from 'react'
import { Controller, type FieldPath, type FieldValues, type UseControllerProps } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { Checkbox } from '@/components/ui/Checkbox'
import { Field, FieldDescription, FieldError } from '@/components/ui/Field'
import { Label } from '@/components/ui/Label'

type CheckboxControlledProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = Omit<ComponentProps<typeof Checkbox>, 'checked' | 'onCheckedChange'> &
  UseControllerProps<TFieldValues, TName> & {
    label: string
    description?: string
  }

function CheckboxControlled<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>({ control, name, label, description, defaultValue, ...props }: CheckboxControlledProps<TFieldValues, TName>) {
  const { t } = useTranslation()
  return (
    <Controller
      control={control}
      defaultValue={defaultValue}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <div className="flex items-center gap-2">
            <Checkbox
              {...props}
              aria-invalid={fieldState.invalid}
              checked={field.value}
              id={field.name}
              onCheckedChange={field.onChange}
            />
            <Label htmlFor={field.name}>{label}</Label>
          </div>
          <FieldDescription>{description}</FieldDescription>
          <FieldError errors={fieldState.error?.message ? [{ message: t(fieldState.error.message) }] : undefined} />
        </Field>
      )}
    />
  )
}

export { CheckboxControlled }
