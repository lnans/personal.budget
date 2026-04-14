import React from 'react'
import { useSearchParams as useSearchParamsRouter } from 'react-router-dom'

type TUseSearchParams = string | null

export function useSearchParams<T extends TUseSearchParams>(queryParam: string) {
  const [searchParams, setSearchParams] = useSearchParamsRouter()
  const value = searchParams.get(queryParam)

  const setValue = React.useCallback(
    (value: T) => {
      setSearchParams((prev) => {
        if (value == null) {
          prev.delete(queryParam)
        } else {
          prev.set(queryParam, value.toString())
        }
        return prev
      })
    },
    [setSearchParams, queryParam]
  )

  return [value, setValue] as const
}
