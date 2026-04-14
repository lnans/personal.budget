export type Problem = {
  type: string
  title: string
  status: number
  detail?: string
  traceId?: string
  errors?: Record<string, { description: string; code: string }[]>
}
