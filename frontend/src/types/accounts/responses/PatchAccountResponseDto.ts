import type { AccountType } from '../enums/AccountType'

export type PatchAccountResponseDto = {
  id: string
  name: string
  bank: string
  type: AccountType
  balance: number
  initialBalance: number
  createdAt: string
  updatedAt: string
}
