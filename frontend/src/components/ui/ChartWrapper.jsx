/**
 * ChartWrapper — fixes Recharts disappearing on click.
 * Recharts loses height when the parent has no explicit pixel height.
 * Wrapping with a div that has a fixed height solves the issue.
 */
export default function ChartWrapper({ height = 220, children }) {
  return (
    <div style={{ width: '100%', height }} className="select-none">
      {children}
    </div>
  )
}
