import { useCallback, useEffect, useState } from 'react'
import { ArrowRight, CalendarDays, CarFront, Check, Gauge, Plus, Search, RotateCcw } from 'lucide-react'
import { api, initials, money, query, toLocalDate, toLocalDateTimeInput } from '../api.js'
import { DataFeedback, Dialog, FormError, PageHeader, StatusPill } from '../components/PageUi.jsx'

const statuses = ['Todos', 'Em viagem', 'Reservado', 'Finalizado', 'Atrasado']

function formatPeriod(start, end) {
  const first = new Date(start)
  const last = new Date(end)
  const month = new Intl.DateTimeFormat('pt-BR', { month: 'short' }).format(last).replace('.', '')
  return `${String(first.getDate()).padStart(2, '0')}–${String(last.getDate()).padStart(2, '0')} ${month}`
}

function RentalHero({ onCreate, onSeeDetails }) {
  const [slide, setSlide] = useState(0)
  const slides = [
    ['Na estrada agora', <>Cada locação abre<br />um novo caminho.</>, 'Acompanhe retiradas, devoluções e a disponibilidade da sua frota em um só lugar.'],
    ['Sua frota em movimento', <>Mais estrada.<br />Menos preocupação.</>, 'Encontre rapidamente o que está em andamento e organize as próximas saídas.'],
    ['Gestão sem complicação', <>A operação flui<br />com tudo no lugar.</>, 'Dados atualizados ajudam sua equipe a cuidar de cada cliente e veículo.'],
  ]
  const item = slides[slide]
  return <section className="hero" aria-label="Destaque da operação">
    <div className="hero-image" /><div className="hero-shade" />
    <div className="hero-content"><span className="hero-eyebrow">{item[0]}</span><h2>{item[1]}</h2><p>{item[2]}</p><button className="hero-link" onClick={onSeeDetails} type="button">Ver locações <ArrowRight size={15} /></button></div>
    <button className="hero-cta" onClick={onCreate} type="button"><Plus size={16} /> Nova locação</button>
    <div className="hero-pagination">{slides.map((_, index) => <button key={index} type="button" className={slide === index ? 'selected' : ''} onClick={() => setSlide(index)} aria-label={`Slide ${index + 1}`}><span>{String(index + 1).padStart(2, '0')}</span><i /></button>)}</div>
  </section>
}

function RentalDialog({ clients, vehicles, onClose, onSave }) {
  const [form, setForm] = useState({ clienteId: '', veiculoId: '', dataInicio: '', dataFimPrevista: '', valorDiaria: '' })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  async function submit(event) {
    event.preventDefault()
    if (!form.clienteId || !form.veiculoId) return setError('Selecione o cliente e o veículo.')
    if (!form.dataInicio || !form.dataFimPrevista) return setError('Informe as datas de retirada e devolução.')
    if (form.dataFimPrevista <= form.dataInicio) return setError('A devolução prevista deve ser posterior à retirada.')
    if (Number(form.valorDiaria) <= 0) return setError('O valor da diária precisa ser maior que zero.')
    setSaving(true); setError('')
    try {
      await onSave({ ClienteId: Number(form.clienteId), VeiculoId: Number(form.veiculoId), DataInicio: new Date(form.dataInicio).toISOString(), DataFimPrevista: new Date(form.dataFimPrevista).toISOString(), ValorDiaria: Number(form.valorDiaria) })
    } catch (err) { setError(err.message) }
    finally { setSaving(false) }
  }

  return <Dialog eyebrow="Nova reserva" title="Criar locação" description="Escolha um cliente, veículo e período para a locação." onClose={onClose}>
    <form className="entity-form" onSubmit={submit} noValidate>
      <label>Cliente<select value={form.clienteId} onChange={(event) => setForm({ ...form, clienteId: event.target.value })}><option value="">Selecione um cliente</option>{clients.map((client) => <option key={client.id} value={client.id}>{client.nome}</option>)}</select></label>
      <label>Veículo<select value={form.veiculoId} onChange={(event) => setForm({ ...form, veiculoId: event.target.value })}><option value="">Selecione um veículo</option>{vehicles.map((vehicle) => <option key={vehicle.id} value={vehicle.id}>{vehicle.modelo} · {vehicle.placa}</option>)}</select></label>
      <div className="form-grid-two"><label>Retirada<input type="datetime-local" value={form.dataInicio} onChange={(event) => setForm({ ...form, dataInicio: event.target.value })} /></label><label>Devolução prevista<input type="datetime-local" value={form.dataFimPrevista} onChange={(event) => setForm({ ...form, dataFimPrevista: event.target.value })} /></label></div>
      <label>Valor da diária<input type="number" min="0.01" step="0.01" value={form.valorDiaria} onChange={(event) => setForm({ ...form, valorDiaria: event.target.value })} placeholder="R$ 0,00" /></label>
      <FormError>{error}</FormError>
      <div className="modal-actions"><button className="cancel-button" type="button" onClick={onClose}>Cancelar</button><button className="submit-button" disabled={saving} type="submit"><Plus size={15} />{saving ? 'Salvando…' : 'Criar locação'}</button></div>
    </form>
  </Dialog>
}

function ReturnDialog({ rental, onClose, onSave }) {
  const [date, setDate] = useState(toLocalDateTimeInput())
  const [km, setKm] = useState(rental.quilometragemInicial)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)
  async function submit(event) {
    event.preventDefault()
    if (Number(km) < rental.quilometragemInicial) return setError(`A quilometragem precisa ser pelo menos ${rental.quilometragemInicial} km.`)
    setSaving(true)
    try { await onSave(rental.id, { DataDevolucao: new Date(date).toISOString(), QuilometragemFinal: Number(km) }) }
    catch (err) { setError(err.message) }
    finally { setSaving(false) }
  }
  return <Dialog eyebrow={`Locação #${rental.id}`} title="Registrar devolução" description={`${rental.veiculo} · ${rental.cliente}`} onClose={onClose}>
    <form className="entity-form" onSubmit={submit} noValidate>
      <label>Data e horário da devolução<input type="datetime-local" value={date} onChange={(event) => setDate(event.target.value)} /></label>
      <label>Quilometragem final<input type="number" min={rental.quilometragemInicial} value={km} onChange={(event) => setKm(event.target.value)} /></label>
      <FormError>{error}</FormError>
      <div className="modal-actions"><button className="cancel-button" type="button" onClick={onClose}>Cancelar</button><button className="submit-button" disabled={saving} type="submit"><Check size={15} />{saving ? 'Salvando…' : 'Confirmar devolução'}</button></div>
    </form>
  </Dialog>
}

export default function Alugueis({ notify }) {
  const [busca, setBusca] = useState('')
  const [situacao, setSituacao] = useState('Todos')
  const [alugueis, setAlugueis] = useState([])
  const [clientes, setClientes] = useState([])
  const [veiculos, setVeiculos] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)

  const load = useCallback(async (signal) => {
    setLoading(true); setError('')
    try {
      const [rentalData, clientData, vehicleData] = await Promise.all([
        api(`/alugueis${query({ busca, situacao: situacao === 'Todos' ? '' : situacao })}`, { signal }),
        api('/clientes', { signal }),
        api('/veiculos', { signal }),
      ])
      setAlugueis(rentalData); setClientes(clientData); setVeiculos(vehicleData)
    } catch (err) { if (err.name !== 'AbortError') setError(err.message) }
    finally { setLoading(false) }
  }, [busca, situacao])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  const emMovimento = alugueis.filter((item) => item.situacao === 'Em viagem').length
  const proximasSaidas = alugueis.filter((item) => item.situacao === 'Reservado').length
  const ocupacao = veiculos.length ? Math.round((emMovimento / veiculos.length) * 100) : 0

  async function createRental(body) {
    await api('/alugueis', { method: 'POST', body })
    setDialog(null); notify('Locação criada.'); await load()
  }
  async function returnRental(id, body) {
    await api(`/alugueis/${id}/devolucao`, { method: 'POST', body })
    setDialog(null); notify('Devolução registrada.'); await load()
  }
  function seeDetails() { document.getElementById('alugueis-table')?.scrollIntoView({ behavior: 'smooth', block: 'start' }) }

  return <div className="section-page rentals-page">
    <PageHeader eyebrow="Gestão de aluguéis" title="Aluguéis" description="Reservas, retiradas e devoluções sem complicação." action={<button className="page-new-rental" type="button" onClick={() => setDialog({ type: 'create' })}><Plus size={16} /> Nova locação</button>} />
    <RentalHero onCreate={() => setDialog({ type: 'create' })} onSeeDetails={seeDetails} />
    <section className="metrics-grid" aria-label="Resumo da operação">
      <article className="metric-card"><span className="metric-index"><CarFront size={15} /></span><div className="metric-copy"><span className="metric-label">Em movimento</span><strong className="metric-value">{emMovimento}</strong><span className="metric-note">veículos em locação</span></div></article>
      <article className="metric-card"><span className="metric-index"><CalendarDays size={15} /></span><div className="metric-copy"><span className="metric-label">Próximas saídas</span><strong className="metric-value">{proximasSaidas}</strong><span className="metric-note">reservas futuras</span></div></article>
      <article className="metric-card"><span className="metric-index"><Gauge size={15} /></span><div className="metric-copy"><span className="metric-label">Ocupação atual</span><strong className="metric-value">{ocupacao}%</strong><span className="metric-note">da frota em movimento</span></div></article>
    </section>

    <section className="table-card management-card rental-list-card" id="alugueis-table">
      <div className="table-heading management-heading"><div><span className="section-kicker">Registros recentes</span><h3>Visão geral</h3></div><div className="management-filters"><label className="table-search"><Search size={15} /><input value={busca} onChange={(event) => setBusca(event.target.value)} placeholder="Cliente, veículo, fabricante ou placa" aria-label="Buscar aluguel por cliente, veículo, fabricante ou placa" /></label><select className="filter-select" value={situacao} onChange={(event) => setSituacao(event.target.value)} aria-label="Filtrar aluguéis por situação">{statuses.map((status) => <option key={status}>{status}</option>)}</select></div></div>
      <DataFeedback loading={loading} error={error} empty={alugueis.length === 0 ? 'Nenhuma locação encontrada para os filtros selecionados.' : ''}>
        <div className="table-scroll"><table className="management-table rentals-table"><thead><tr><th>Reserva / cliente</th><th>Veículo</th><th>Período</th><th>Valor total</th><th>Situação</th><th className="actions-col">Ação</th></tr></thead><tbody>
          {alugueis.map((item) => <tr key={item.id}><td><div className="reservation-cell"><span className="customer-avatar avatar-sand">{initials(item.cliente)}</span><div className="reservation-copy"><strong>#{item.id} · {item.cliente}</strong><small>Retirada {toLocalDate(item.dataInicio)}</small></div></div></td><td><strong className="table-primary">{item.veiculo}</strong><small className="table-subline">{item.placa}</small></td><td>{formatPeriod(item.dataInicio, item.dataFimPrevista)}</td><td>{item.valorTotal == null ? money(item.valorDiaria) + '/dia' : money(item.valorTotal)}</td><td><StatusPill>{item.situacao}</StatusPill></td><td className="actions-col">{item.situacao === 'Em viagem' || item.situacao === 'Atrasado' ? <button className="return-action" type="button" onClick={() => setDialog({ type: 'return', item })}><RotateCcw size={13} /> Devolver</button> : <span className="muted-dash">—</span>}</td></tr>)}
        </tbody></table></div><div className="table-footer"><span>{alugueis.length} locações encontradas</span><span>Atualizado do banco da locadora</span></div>
      </DataFeedback>
    </section>
    {dialog?.type === 'create' && <RentalDialog clients={clientes} vehicles={veiculos} onClose={() => setDialog(null)} onSave={createRental} />}
    {dialog?.type === 'return' && <ReturnDialog rental={dialog.item} onClose={() => setDialog(null)} onSave={returnRental} />}
  </div>
}
