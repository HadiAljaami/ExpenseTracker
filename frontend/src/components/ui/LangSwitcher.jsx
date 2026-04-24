import { useTranslation } from 'react-i18next'
import { clsx } from 'clsx'

export default function LangSwitcher({ className }) {
  const { i18n } = useTranslation()
  const isAr = i18n.language === 'ar'

  const toggle = () => i18n.changeLanguage(isAr ? 'en' : 'ar')

  return (
    <button
      onClick={toggle}
      className={clsx(
        'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium',
        'border border-gray-200 hover:bg-gray-50 transition-colors',
        'text-gray-600 hover:text-gray-900',
        className
      )}
      title={isAr ? 'Switch to English' : 'التبديل إلى العربية'}
    >
      <span className="text-base leading-none">{isAr ? '🇺🇸' : '🇸🇦'}</span>
      <span>{isAr ? 'EN' : 'عربي'}</span>
    </button>
  )
}
