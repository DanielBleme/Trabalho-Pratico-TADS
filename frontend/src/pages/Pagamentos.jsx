import React, { useCallback, useEffect, useState } from 'react'
import { CreditCard, Pencil, Plus, Search, Trash2, Wallet } from 'lucide-react'
import { api, money, query, toLocalDateTimeInput } from '../api.js'
import { DataFeedback, Dialog, FormError, PageCard, PageHeader, StatusPill } from '../components/PageUi.jsx'

const emptyPayment = { aluguelId: '', dataPagamento: toLocalDateTimeInput(), valorPago: '', metodoPagamento: 'Pix', status: 'Pago' }

export default function Pagamentos({ notify }) {
  const [busca, setBusca] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [inicio, setInicio] = useState('')
  const [fim, setFim] = useState('')
  const [pagamentos, setPagamentos] = useState([])
  const [alugueis, setAlugueis] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [form, setForm] = useState(emptyPayment)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async (signal) => {
    setLoading(true)
    setError('')
    try {
      const [paymentData, rentalData] = await Promise.all([
        api(`/pagamentos${query({ busca, status: statusFilter, inicio, fim })}`, { signal }),
        api('/alugueis', { signal }),
      ])
      setPagamentos(paymentData)
      setAlugueis(rentalData)
    } catch (err) { if (err.name !== 'AbortError') setError(err.message) }
    finally { setLoading(false) }
  }, [busca, statusFilter, inicio, fim])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  function openCreate() { setForm({ ...emptyPayment, dataPagamento: toLocalDateTimeInput(), aluguelId: alugueis[0]?.id || '' }); setFormError(''); setDialog({ mode: 'create' }) }
  function openEdit(item) { setForm({ aluguelId: item.aluguelId, dataPagamento: toLocalDateTimeInput(new Date(item.dataPagamento)), valorPago: item.valorPago, metodoPagamento: item.metodoPagamento, status: item.status }); setFormError(''); setDialog({ mode: 'edit', item }) }

  async function save(event) {
    event.preventDefault()
    if (!form.aluguelId) return setFormError('Selecione o aluguel relacionado.')
    if (!Number(form.valorPago) || Number(form.valorPago) <= 0) return setFormError('O valor precisa ser maior que zero.')
    setSaving(true)
    setFormError('')
    try {
      const body = { AluguelId: Number(form.aluguelId), DataPagamento: new Date(form.dataPagamento).toISOString(), ValorPago: Number(form.valorPago), MetodoPagamento: form.metodoPagamento, Status: form.status }
      if (dialog.mode === 'edit') await api(`/pagamentos/${dialog.item.id}`, { method: 'PUT', body })
      else await api('/pagamentos', { method: 'POST', body })
      setDialog(null)
      notify(dialog.mode === 'edit' ? 'Pagamento atualizado.' : 'Pagamento registrado.')
      await load()
    } catch (err) { setFormError(err.message) }
    finally { setSaving(false) }
  }

  async function remove(item) {
    if (!window.confirm(`Excluir o pagamento ${money(item.valorPago)} de ${item.cliente}?`)) return
    try { await api(`/pagamentos/${item.id}`, { method: 'DELETE' }); notify('Pagamento removido.'); await load() }
    catch (err) { setError(err.message) }
  }

  const total = pagamentos.reduce((sum, item) => sum + item.valorPago, 0)
  const pendentes = pagamentos.filter((item) => item.status.toLowerCase() !== 'pago').length

  return (
    <div className="section-page">
      <PageHeader eyebrow="Financeiro" title="Pagamentos" description="Acompanhe valores recebidos, pendências e a forma de pagamento de cada locação." action={<button className="page-new-rental" type="button" onClick={openCreate}><Plus size={16} /> Registrar pagamento</button>} />
      <div className="summary-strip">
        <div><span className="summary-icon"><Wallet size={16} /></span><span><small>Total no período filtrado</small><strong>{money(total)}</strong></span></div>
        <div><span className="summary-icon"><CreditCard size={16} /></span><span><small>Pagamentos encontrados</small><strong>{pagamentos.length}</strong></span></div>
        <div><span className="summary-icon summary-warn"><CreditCard size={16} /></span><span><small>Com status pendente/parcial</small><strong>{pendentes}</strong></span></div>
      </div>
      <PageCard>
        <div className="table-heading management-heading"><div><span className="section-kicker">Movimentações</span><h3>Todos os pagamentos</h3></div><div className="management-filters payment-filters"><label className="table-search"><Search size={15} /><input value={busca} onChange={(event) => setBusca(event.target.value)} placeholder="Cliente, veículo ou método" aria-label="Buscar pagamentos" /></label><select className="filter-select" value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} aria-label="Filtrar por status"><option value="">Todos os status</option><option>Pago</option><option>Parcial</option><option>Pendente</option></select></div></div>
        <div className="date-filter-row"><label>De<input type="date" value={inicio} onChange={(event) => setInicio(event.target.value)} /></label><label>Até<input type="date" value={fim} onChange={(event) => setFim(event.target.value)} /></label><button className="clear-filters" type="button" onClick={() => { setInicio(''); setFim(''); setStatusFilter(''); setBusca('') }}>Limpar filtros</button></div>
        <DataFeedback loading={loading} error={error} empty={pagamentos.length === 0 ? 'Nenhum pagamento encontrado para os filtros selecionados.' : ''}>
          <div className="table-scroll"><table className="management-table"><thead><tr><th>Data</th><th>Cliente</th><th>Locação / veículo</th><th>Método</th><th>Valor</th><th>Status</th><th className="actions-col">Ações</th></tr></thead><tbody>
            {pagamentos.map((item) => <tr key={item.id}><td>{new Intl.DateTimeFormat('pt-BR').format(new Date(item.dataPagamento))}</td><td><strong className="table-primary">{item.cliente}</strong></td><td>{item.veiculo}<span className="plate-separator"> · </span>{item.placa}</td><td><span className="method-chip">{item.metodoPagamento}</span></td><td><strong className="table-primary">{money(item.valorPago)}</strong></td><td><StatusPill>{item.status}</StatusPill></td><td className="actions-col"><button className="icon-action" title="Editar" onClick={() => openEdit(item)}><Pencil size={15} /></button><button className="icon-action danger-action" title="Excluir" onClick={() => remove(item)}><Trash2 size={15} /></button></td></tr>)}
          </tbody></table></div><div className="table-footer"><span>{pagamentos.length} pagamentos encontrados</span></div>
        </DataFeedback>
      </PageCard>

      {dialog && <Dialog eyebrow={dialog.mode === 'edit' ? 'Editar lançamento' : 'Novo lançamento'} title={dialog.mode === 'edit' ? 'Editar pagamento' : 'Registrar pagamento'} description="O lançamento será vinculado a um aluguel existente." onClose={() => setDialog(null)}>
        <form className="entity-form" onSubmit={save} noValidate>
          <label>Aluguel<select value={form.aluguelId} onChange={(event) => setForm({ ...form, aluguelId: event.target.value })}><option value="">Selecione um aluguel</option>{alugueis.map((item) => <option key={item.id} value={item.id}>#{item.id} · {item.cliente} · {item.veiculo}</option>)}</select></label>
          <div className="form-grid-two"><label>Valor pago<input type="number" min="0.01" step="0.01" value={form.valorPago} onChange={(event) => setForm({ ...form, valorPago: event.target.value })} placeholder="0,00" /></label><label>Data do pagamento<input type="datetime-local" value={form.dataPagamento} onChange={(event) => setForm({ ...form, dataPagamento: event.target.value })} /></label></div>
          <div className="form-grid-two"><label>Método<select value={form.metodoPagamento} onChange={(event) => setForm({ ...form, metodoPagamento: event.target.value })}><option>Pix</option><option>Cartão de crédito</option><option>Cartão de débito</option><option>Dinheiro</option><option>Transferência</option></select></label><label>Status<select value={form.status} onChange={(event) => setForm({ ...form, status: event.target.value })}><option>Pago</option><option>Parcial</option><option>Pendente</option></select></label></div>
          <FormError>{formError}</FormError>
          <div className="modal-actions"><button className="cancel-button" type="button" onClick={() => setDialog(null)}>Cancelar</button><button className="submit-button" disabled={saving} type="submit">{saving ? 'Salvando…' : 'Salvar pagamento'}</button></div>
        </form>
      </Dialog>}
    </div>
  )
}
