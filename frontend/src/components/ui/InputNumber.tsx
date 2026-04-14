import { ChevronDown, ChevronUp } from 'lucide-react'
import { startTransition, useCallback, useEffect, useRef, useState } from 'react'
import { NumericFormat, type NumericFormatProps } from 'react-number-format'

import { Button } from './Button'
import { Input } from './Input'

export type InputNumberProps = {
  stepper?: number
  thousandSeparator?: string
  placeholder?: string
  defaultValue?: number
  min?: number
  max?: number
  value?: number // Controlled value
  suffix?: string
  prefix?: string
  onValueChange?: (value: number | undefined) => void
  fixedDecimalScale?: boolean
  decimalScale?: number
} & Omit<NumericFormatProps, 'value' | 'onValueChange'>

function InputNumber({
  ref,
  stepper,
  thousandSeparator,
  placeholder,
  defaultValue,
  min = -Infinity,
  max = Infinity,
  onValueChange,
  fixedDecimalScale = false,
  decimalScale = 0,
  suffix,
  prefix,
  value: controlledValue,
  ...props
}: InputNumberProps & { ref?: React.RefObject<HTMLInputElement | null> }) {
  const internalRef = useRef<HTMLInputElement>(null) // Create an internal ref
  const combinedRef = ref ?? internalRef // Use provided ref or internal ref
  const [value, setValue] = useState<number | undefined>(controlledValue ?? defaultValue)

  const handleIncrement = useCallback(() => {
    setValue((prev) => (prev === undefined ? (stepper ?? 1) : Math.min(prev + (stepper ?? 1), max)))
  }, [stepper, max])

  const handleDecrement = useCallback(() => {
    setValue((prev) => (prev === undefined ? -(stepper ?? 1) : Math.max(prev - (stepper ?? 1), min)))
  }, [stepper, min])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const inputEl = (combinedRef as React.RefObject<HTMLInputElement> | undefined)?.current
      if (inputEl && document.activeElement === inputEl) {
        if (e.key === 'ArrowUp') {
          handleIncrement()
        } else if (e.key === 'ArrowDown') {
          handleDecrement()
        }
      }
    }

    window.addEventListener('keydown', handleKeyDown)
    return () => {
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [handleIncrement, handleDecrement, combinedRef])

  useEffect(() => {
    if (controlledValue !== undefined) {
      startTransition(() => {
        setValue(controlledValue)
      })
    }
  }, [controlledValue])

  const handleChange = (values: { value: string; floatValue: number | undefined }) => {
    const newValue = values.floatValue ?? undefined
    setValue(newValue)
    if (onValueChange) {
      onValueChange(newValue)
    }
  }

  const handleBlur = () => {
    if (value !== undefined) {
      if (value < min) {
        setValue(min)
        if (onValueChange) onValueChange(min)
      } else if (value > max) {
        setValue(max)
        if (onValueChange) onValueChange(max)
      }
    }
  }

  return (
    <div className="flex items-center">
      <NumericFormat
        valueIsNumericString
        allowNegative={min < 0}
        className="[appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none rounded-r-none relative"
        customInput={Input}
        decimalScale={decimalScale}
        fixedDecimalScale={fixedDecimalScale}
        getInputRef={combinedRef} // Use combined ref
        max={max}
        min={min}
        placeholder={placeholder}
        prefix={prefix}
        suffix={suffix}
        thousandSeparator={thousandSeparator}
        value={value}
        onBlur={handleBlur}
        onValueChange={handleChange}
        {...props}
      />
      <div className="flex flex-col">
        <Button
          aria-label="Increase value"
          className="px-2 h-[18px] w-8 rounded-l-none rounded-br-none border-input border-l-0 border-b-[0.5px] focus-visible:relative"
          disabled={value === max}
          type="button"
          variant="outline"
          onClick={handleIncrement}
        >
          <ChevronUp size={15} />
        </Button>
        <Button
          aria-label="Decrease value"
          className="px-2 h-[18px] w-8 rounded-l-none rounded-tr-none border-input border-l-0 border-t-[0.5px] focus-visible:relative"
          disabled={value === min}
          type="button"
          variant="outline"
          onClick={handleDecrement}
        >
          <ChevronDown size={15} />
        </Button>
      </div>
    </div>
  )
}

export { InputNumber }
