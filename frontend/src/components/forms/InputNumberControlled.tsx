import { Controller, type FieldPath, type FieldValues, type UseControllerProps } from 'react-hook-form'
import { useTranslation } from 'react-i18next'

import { Field, FieldDescription, FieldError, FieldLabel } from '@/components/ui/Field'
import { InputNumber, type InputNumberProps } from '@/components/ui/InputNumber'

type InputNumberControlledProps<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
> = Omit<InputNumberProps, 'value' | 'onValueChange'> &
  UseControllerProps<TFieldValues, TName> & {
    label: string
    description?: string
    placeholder?: string
  }

function InputNumberControlled<
  TFieldValues extends FieldValues = FieldValues,
  TName extends FieldPath<TFieldValues> = FieldPath<TFieldValues>,
>({ control, name, label, description, placeholder, defaultValue, ...props }: InputNumberControlledProps<TFieldValues, TName>) {
  const { t } = useTranslation()
  return (
    <Controller
      control={control}
      defaultValue={defaultValue}
      name={name}
      render={({ field, fieldState }) => (
        <Field data-invalid={fieldState.invalid}>
          <FieldLabel htmlFor={field.name}>{label}</FieldLabel>
          <InputNumber
            {...props}
            aria-invalid={fieldState.invalid}
            id={field.name}
            placeholder={placeholder}
            value={field.value}
            onValueChange={(value) => field.onChange(value)}
          />
          <FieldDescription>{description}</FieldDescription>
          <FieldError errors={fieldState.error?.message ? [{ message: t(fieldState.error.message) }] : undefined} />
        </Field>
      )}
    />
  )
}

export { InputNumberControlled }
