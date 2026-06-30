import { es } from 'date-fns/locale'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import type { ComponentProps } from 'react'
import { DayPicker } from 'react-day-picker'
import { cn } from '@/shared/utils/cn'
import { buttonVariants } from './button-variants'

export type CalendarProps = ComponentProps<typeof DayPicker>

// Modifier class names intentionally NOT overridden here — they keep their
// rdp-* defaults (rdp-selected, rdp-range_start, rdp-range_middle, etc.).
// All interactive styling is handled in index.css via .cal-cell.rdp-* selectors.
export function Calendar({
  className,
  classNames,
  showOutsideDays = true,
  ...props
}: Readonly<CalendarProps>) {
  return (
    <DayPicker
      locale={es}
      showOutsideDays={showOutsideDays}
      className={cn('p-3', className)}
      classNames={{
        months:          'flex flex-col sm:flex-row gap-5',
        month:           'flex flex-col gap-3',
        month_caption:   'flex justify-center relative items-center py-1',
        caption_label:   'text-[13px] font-semibold capitalize text-gray-700',
        nav:             'flex items-center gap-1',
        button_previous: cn(
          buttonVariants({ variant: 'ghost', size: 'icon' }),
          'absolute left-0 h-7 w-7 opacity-50 hover:opacity-100',
        ),
        button_next: cn(
          buttonVariants({ variant: 'ghost', size: 'icon' }),
          'absolute right-0 h-7 w-7 opacity-50 hover:opacity-100',
        ),
        month_grid: 'w-full border-collapse',
        weekdays:   'flex',
        weekday:    'w-8 text-center text-[11px] font-semibold uppercase tracking-wide text-gray-400',
        weeks:      'mt-1',
        week:       'flex w-full mt-0.5',
        // Marker classes only — rdp-* modifier classes are added on top by DayPicker
        day:        'cal-cell',
        day_button: 'cal-btn',
        hidden:     'invisible',
        ...classNames,
      }}
      components={{
        Chevron: ({ orientation }) =>
          orientation === 'left'
            ? <ChevronLeft className="h-3.5 w-3.5" />
            : <ChevronRight className="h-3.5 w-3.5" />,
      }}
      {...props}
    />
  )
}
