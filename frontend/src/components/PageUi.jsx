import { useEffect } from 'react'
import { AlertCircle, Check, LoaderCircle, X } from 'lucide-react'

export function PageHeader({ eyebrow, title, description, action }) {
  return (
    <div className="page-heading">
      <div><span className="section-kicker">{eyebrow}</span><h1>{title}</h1><p>{description}</p></div>
      {action}
    </div>
  )
}

export function PageCard({ children, className = '' }) {
  return <section className={`table-card management-card ${className}`}>{children}</section>
}

export function DataFeedback({ loading, error, empty, children }) {
  if (loading) return <div className="data-feedback"><LoaderCircle className="spin" size={18} />Carregando dados do servidor…</div>
  if (error) return <div className="data-feedback data-error"><AlertCircle size={17} /><span>{error}</span></div>
  if (empty) return <div className="data-feedback">{empty}</div>
  return children
}

export function Dialog({ title, eyebrow = 'Cadastro', description, onClose, children, wide = false }) {
  useEffect(() => {
    function onKeyDown(event) {
      if (event.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [onClose])

  return (
    <div className="modal-backdrop" onMouseDown={(event) => event.target === event.currentTarget && onClose()}>
      <section className={`rental-modal ${wide ? 'wide-modal' : ''}`} role="dialog" aria-modal="true" aria-labelledby="dialog-title">
        <div className="modal-heading"><div><span className="section-kicker">{eyebrow}</span><h2 id="dialog-title">{title}</h2></div><button className="modal-close" type="button" onClick={onClose} aria-label="Fechar"><X size={18} /></button></div>
        {description && <p className="modal-intro">{description}</p>}
        {children}
      </section>
    </div>
  )
}

export function FormError({ children }) {
  return children ? <p className="form-error" role="alert"><AlertCircle size={14} />{children}</p> : null
}

export function StatusPill({ children }) {
  const normalized = String(children || '').toLowerCase().replaceAll(' ', '-').normalize('NFD').replace(/[\u0300-\u036f]/g, '')
  return <span className={`status-pill status-${normalized}`}><i />{children}</span>
}

export function Toast({ message, onDismiss }) {
  useEffect(() => {
    if (!message) return undefined
    const timeout = window.setTimeout(onDismiss, 3200)
    return () => window.clearTimeout(timeout)
  }, [message, onDismiss])
  return message ? <div className="toast-message" role="status"><Check size={16} />{message}</div> : null
}
