import { useCallback, useEffect, useState } from 'react'
import { CarFront, Gauge, Pencil, Plus, Search, Trash2 } from 'lucide-react'
import { api, query } from '../api.js'
import { DataFeedback, Dialog, FormError, PageCard, PageHeader } from '../components/PageUi.jsx'

const emptyVehicle = { modelo: '', anoFabricacao: new Date().getFullYear(), placa: '', quilometragem: 0, fabricanteId: '' }

export default function Veiculos({ notify }) {
  const [busca, setBusca] = useState('')
  const [fabricanteId, setFabricanteId] = useState('')
  const [veiculos, setVeiculos] = useState([])
  const [fabricantes, setFabricantes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [form, setForm] = useState(emptyVehicle)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async (signal) => {
    setLoading(true)
    setError('')
    try {
      const [vehicleData, makerData] = await Promise.all([
        api(`/veiculos${query({ busca, fabricanteId })}`, { signal }),
        api('/fabricantes', { signal }),
      ])
      setVeiculos(vehicleData)
      setFabricantes(makerData)
    } catch (err) {
      if (err.name !== 'AbortError') setError(err.message)
    } finally { setLoading(false) }
  }, [busca, fabricanteId])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  function openCreate() { setForm({ ...emptyVehicle, fabricanteId: fabricantes[0]?.id || '' }); setFormError(''); setDialog({ mode: 'create' }) }
  function openEdit(item) { setForm({ modelo: item.modelo, anoFabricacao: item.anoFabricacao, placa: item.placa, quilometragem: item.quilometragem, fabricanteId: item.fabricanteId }); setFormError(''); setDialog({ mode: 'edit', item }) }

  async function save(event) {
    event.preventDefault()
    if (!form.modelo.trim()) return setFormError('Informe o modelo do veículo.')
    if (!/^[A-Za-z0-9-]{7,10}$/.test(form.placa.trim())) return setFormError('Informe uma placa válida, com 7 a 10 caracteres.')
    if (!form.fabricanteId) return setFormError('Cadastre e selecione um fabricante.')
    setSaving(true)
    setFormError('')
    try {
      const body = { Modelo: form.modelo.trim(), AnoFabricacao: Number(form.anoFabricacao), Placa: form.placa.trim().toUpperCase(), Quilometragem: Number(form.quilometragem), FabricanteId: Number(form.fabricanteId) }
      if (dialog.mode === 'edit') await api(`/veiculos/${dialog.item.id}`, { method: 'PUT', body })
      else await api('/veiculos', { method: 'POST', body })
      setDialog(null)
      notify(dialog.mode === 'edit' ? 'Veículo atualizado.' : 'Veículo cadastrado.')
      await load()
    } catch (err) { setFormError(err.message) }
    finally { setSaving(false) }
  }

  async function remove(item) {
    if (!window.confirm(`Excluir o veículo ${item.modelo} · ${item.placa}?`)) return
    try {
      await api(`/veiculos/${item.id}`, { method: 'DELETE' })
      notify('Veículo removido.')
      await load()
    } catch (err) { setError(err.message) }
  }

  return (
    <div className="section-page">
      <PageHeader eyebrow="Gestão da frota" title="Veículos" description="Encontre veículos por modelo, placa ou fabricante e mantenha a frota atualizada." action={<button className="page-new-rental" type="button" onClick={openCreate}><Plus size={16} /> Novo veículo</button>} />
      <div className="summary-strip">
        <div><span className="summary-icon"><CarFront size={16} /></span><span><small>Veículos encontrados</small><strong>{veiculos.length}</strong></span></div>
        <div><span className="summary-icon"><Gauge size={16} /></span><span><small>Quilometragem média</small><strong>{veiculos.length ? `${Math.round(veiculos.reduce((sum, item) => sum + item.quilometragem, 0) / veiculos.length).toLocaleString('pt-BR')} km` : '—'}</strong></span></div>
      </div>
      <PageCard>
        <div className="table-heading management-heading"><div><span className="section-kicker">Frota cadastrada</span><h3>Todos os veículos</h3></div><div className="management-filters"><label className="table-search"><Search size={15} /><input value={busca} onChange={(event) => setBusca(event.target.value)} placeholder="Modelo, placa ou fabricante" aria-label="Buscar modelo, placa ou fabricante" /></label><select className="filter-select" value={fabricanteId} onChange={(event) => setFabricanteId(event.target.value)} aria-label="Filtrar por fabricante"><option value="">Todos os fabricantes</option>{fabricantes.map((item) => <option key={item.id} value={item.id}>{item.nome}</option>)}</select></div></div>
        <DataFeedback loading={loading} error={error} empty={veiculos.length === 0 ? 'Nenhum veículo encontrado para esses filtros.' : ''}>
          <div className="table-scroll"><table className="management-table"><thead><tr><th>Veículo</th><th>Fabricante</th><th>Ano</th><th>Placa</th><th>Quilometragem</th><th className="actions-col">Ações</th></tr></thead><tbody>
            {veiculos.map((item) => <tr key={item.id}><td><div className="entity-cell"><span className="entity-avatar"><CarFront size={15} /></span><strong>{item.modelo}</strong></div></td><td>{item.fabricante}</td><td>{item.anoFabricacao}</td><td><span className="plate-chip">{item.placa}</span></td><td>{item.quilometragem.toLocaleString('pt-BR')} km</td><td className="actions-col"><button className="icon-action" title="Editar" onClick={() => openEdit(item)}><Pencil size={15} /></button><button className="icon-action danger-action" title="Excluir" onClick={() => remove(item)}><Trash2 size={15} /></button></td></tr>)}
          </tbody></table></div><div className="table-footer"><span>{veiculos.length} veículos encontrados</span></div>
        </DataFeedback>
      </PageCard>

      {dialog && <Dialog eyebrow={dialog.mode === 'edit' ? 'Editar cadastro' : 'Cadastro da frota'} title={dialog.mode === 'edit' ? 'Editar veículo' : 'Adicionar veículo'} description="Vincule o veículo a um fabricante cadastrado." onClose={() => setDialog(null)}>
        <form className="entity-form" onSubmit={save} noValidate>
          <label>Modelo<input autoFocus maxLength="100" value={form.modelo} onChange={(event) => setForm({ ...form, modelo: event.target.value })} placeholder="Ex.: Corolla" /></label>
          <div className="form-grid-two"><label>Fabricante<select value={form.fabricanteId} onChange={(event) => setForm({ ...form, fabricanteId: event.target.value })}><option value="">Selecione</option>{fabricantes.map((item) => <option key={item.id} value={item.id}>{item.nome}</option>)}</select></label><label>Ano<input type="number" min="1950" max="2100" value={form.anoFabricacao} onChange={(event) => setForm({ ...form, anoFabricacao: event.target.value })} /></label></div>
          <div className="form-grid-two"><label>Placa<input maxLength="10" value={form.placa} onChange={(event) => setForm({ ...form, placa: event.target.value.toUpperCase() })} placeholder="ABC1D23" /></label><label>Quilometragem<input type="number" min="0" value={form.quilometragem} onChange={(event) => setForm({ ...form, quilometragem: event.target.value })} /></label></div>
          <FormError>{formError}</FormError>
          <div className="modal-actions"><button className="cancel-button" type="button" onClick={() => setDialog(null)}>Cancelar</button><button className="submit-button" disabled={saving} type="submit">{saving ? 'Salvando…' : 'Salvar veículo'}</button></div>
        </form>
      </Dialog>}
    </div>
  )
}
