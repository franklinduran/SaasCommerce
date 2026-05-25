import { useMutation } from '@tanstack/react-query'
import {
  getImportTemplate,
  productImportApi,
  type ImportProductsResponse,
} from '@/modules/products/services/productImportApi'

export function useProductImport() {
  const importMutation = useMutation<ImportProductsResponse, Error, File>({
    mutationFn: (file: File) => productImportApi.importCsv(file),
  })

  const templateMutation = useMutation({
    mutationFn: getImportTemplate,
  })

  return {
    downloadTemplate: templateMutation.mutateAsync,
    errorMessage: importMutation.error?.message,
    importCsv: importMutation.mutateAsync,
    isDownloadingTemplate: templateMutation.isPending,
    isImporting: importMutation.isPending,
    reset: importMutation.reset,
    result: importMutation.data,
  }
}
