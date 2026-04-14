import type { AccountType } from '../enums/AccountType'

export type DeleteAccountResponseDto = {
  id: string
  name: string
  bank: string
  type: AccountType
  balance: number
  initialBalance: number
  createdAt: string
  updatedAt: string
  deletedAt: string
}
