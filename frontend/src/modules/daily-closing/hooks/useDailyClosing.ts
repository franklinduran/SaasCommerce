import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { dailyClosingApi } from '@/modules/daily-closing/services/dailyClosingApi'

export const dailyClosingQueryKeys = {
  list: (params?: object) => ['daily-closing', 'list', params] as const,
  detail: (id: string) => ['daily-closing', 'detail', id] as const,
  preview: (date: string, branchId: string) =>
    ['daily-closing', 'preview', date, branchId] as const,
}

export function useDailyClosingList(params?: {
  branchId?: string
  dateFrom?: string
  dateTo?: string
  page?: number
  pageSize?: number
}) {
  return useQuery({
    queryKey: dailyClosingQueryKeys.list(params),
    queryFn: () => dailyClosingApi.getList(params ?? {}),
    staleTime: 1000 * 30,
  })
}

export function useDailyClosingDetail(id: string) {
  return useQuery({
    queryKey: dailyClosingQueryKeys.detail(id),
    queryFn: () => dailyClosingApi.getDetail(id),
    enabled: Boolean(id),
    staleTime: 1000 * 60,
  })
}

export function useDailyClosingPreview(date: string, branchId: string) {
  return useQuery({
    queryKey: dailyClosingQueryKeys.preview(date, branchId),
    queryFn: () => dailyClosingApi.preview(date, branchId),
    enabled: Boolean(date) && Boolean(branchId),
    staleTime: 1000 * 30,
    retry: false,
  })
}

export function useCreateDailyClosing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (params: { date: string; branchId: string; notes?: string }) =>
      dailyClosingApi.create(params),
    onSuccess: (closing) => {
      queryClient.setQueryData(dailyClosingQueryKeys.detail(closing.id), closing)
      void queryClient.invalidateQueries({ queryKey: ['daily-closing', 'list'] })
      void queryClient.invalidateQueries({ queryKey: ['daily-closing', 'preview'] })
    },
  })
}

export function useCloseDailyClosing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      closingId,
      params,
    }: {
      closingId: string
      params: { cashCounted: number; notes?: string }
    }) => dailyClosingApi.close(closingId, params),
    onSuccess: (closing) => {
      queryClient.setQueryData(dailyClosingQueryKeys.detail(closing.id), closing)
      void queryClient.invalidateQueries({ queryKey: ['daily-closing', 'list'] })
    },
  })
}
