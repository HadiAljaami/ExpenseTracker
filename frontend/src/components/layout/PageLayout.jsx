import Topbar from './Topbar'

export default function PageLayout({ title, actions, children }) {
  return (
    <div className="flex flex-col h-full min-h-0">
      <Topbar title={title} actions={actions} />
      <main className="flex-1 overflow-y-auto p-4 lg:p-6">
        {children}
      </main>
    </div>
  )
}
