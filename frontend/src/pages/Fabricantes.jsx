import { useCallback, useEffect, useState } from 'react'
import { Building2, Pencil, Plus, Search, Trash2, Truck } from 'lucide-react'
import { api, query } from '../api.js'
import { DataFeedback, Dialog, FormError, PageCard, PageHeader } from '../components/PageUi.jsx'

function formatCnpj(value) {
  const digits = value.replace(/\D/g, '').slice(0, 14)
  return digits
    .replace(/^(\d{2})(\d)/, '$1.$2')
    .replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1/$2')
    .replace(/(\d{4})(\d)/, '$1-$2')
}

const blankForm = { nome: '', cnpj: '' }

export default function Fabricantes({ notify }) {
  const [busca, setBusca] = useState('')
  const [fabricantes, setFabricantes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [form, setForm] = useState(blankForm)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async (signal) => {
    setLoading(true)
    setError('')
    try {
      const data = await api(`/fabricantes${query({ busca })}`, { signal })
      setFabricantes(data)
    } catch (err) {
      if (err.name !== 'AbortError') setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [busca])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  function openCreate() { setForm(blankForm); setFormError(''); setDialog({ mode: 'create' }) }
  function openEdit(item) { setForm({ nome: item.nome, cnpj: item.cnpj }); setFormError(''); setDialog({ mode: 'edit', item }) }

  async function save(event) {
    event.preventDefault()
    if (form.nome.trim().length < 2) return setFormError('Informe o nome do fabricante.')
    if (form.cnpj.replace(/\D/g, '').length !== 14) return setFormError('Informe um CNPJ com 14 números.')
    setSaving(true)
    setFormError('')
    try {
      const body = { Nome: form.nome.trim(), CNPJ: form.cnpj.trim() }
      if (dialog.mode === 'edit') await api(`/fabricantes/${dialog.item.id}`, { method: 'PUT', body })
      else await api('/fabricantes', { method: 'POST', body })
      setDialog(null)
      notify(dialog.mode === 'edit' ? 'Fabricante atualizado.' : 'Fabricante cadastrado.')
      await load()
    } catch (err) {
      setFormError(err.message)
    } finally {
      setSaving(false)
    }
  }

  async function remove(item) {
    if (!window.confirm(`Excluir o fabricante ${item.nome}?`)) return
    try {
      await api(`/fabricantes/${item.id}`, { method: 'DELETE' })
      notify('Fabricante removido.')
      await load()
    } catch (err) { setError(err.message) }
  }

  return (
    <div className="section-page">
      <PageHeader eyebrow="Cadastro da frota" title="Fabricantes" description="Organize as marcas e acompanhe os veículos de cada fabricante." action={<button className="page-new-rental" type="button" onClick={openCreate}><Plus size={16} /> Novo fabricante</button>} />
      <div className="summary-strip">
        <div><span className="summary-icon"><Building2 size={16} /></span><span><small>Fabricantes cadastrados</small><strong>{fabricantes.length}</strong></span></div>
        <div><span className="summary-icon"><Truck size={16} /></span><span><small>Veículos vinculados</small><strong>{fabricantes.reduce((sum, item) => sum + item.totalVeiculos, 0)}</strong></span></div>
      </div>
      <PageCard>
        <div className="table-heading management-heading"><div><span className="section-kicker">Base de marcas</span><h3>Todos os fabricantes</h3></div><label className="table-search"><Search size={15} /><input value={busca} onChange={(event) => setBusca(event.target.value)} placeholder="Buscar por nome ou CNPJ" aria-label="Buscar por nome ou CNPJ" /></label></div>
        <DataFeedback loading={loading} error={error} empty={fabricantes.length === 0 ? 'Nenhum fabricante cadastrado. Adicione o primeiro para começar.' : ''}>
          <div className="table-scroll"><table className="management-table"><thead><tr><th>Fabricante</th><th>CNPJ</th><th>Veículos</th><th className="actions-col">Ações</th></tr></thead><tbody>
            {fabricantes.map((item) => <tr key={item.id}><td><div className="entity-cell"><span className="entity-avatar"><Building2 size={15} /></span><strong>{item.nome}</strong></div></td><td>{item.cnpj}</td><td><span className="count-chip">{item.totalVeiculos} veículos</span></td><td className="actions-col"><button className="icon-action" title="Editar" onClick={() => openEdit(item)}><Pencil size={15} /></button><button className="icon-action danger-action" title="Excluir" onClick={() => remove(item)}><Trash2 size={15} /></button></td></tr>)}
          </tbody></table></div>
          <div className="table-footer"><span>{fabricantes.length} fabricantes encontrados</span></div>
        </DataFeedback>
      </PageCard>

      {dialog && <Dialog eyebrow={dialog.mode === 'edit' ? 'Editar cadastro' : 'Nova marca'} title={dialog.mode === 'edit' ? 'Editar fabricante' : 'Cadastrar fabricante'} description="Os dados serão salvos no banco da locadora." onClose={() => setDialog(null)}>
        <form className="entity-form" onSubmit={save} noValidate>
          <label>Nome do fabricante<input autoFocus maxLength="120" value={form.nome} onChange={(event) => setForm({ ...form, nome: event.target.value })} placeholder="Ex.: Toyota" /></label>
          <label>CNPJ<input inputMode="numeric" value={form.cnpj} onChange={(event) => setForm({ ...form, cnpj: formatCnpj(event.target.value) })} placeholder="00.000.000/0000-00" /></label>
          <FormError>{formError}</FormError>
          <div className="modal-actions"><button className="cancel-button" type="button" onClick={() => setDialog(null)}>Cancelar</button><button className="submit-button" disabled={saving} type="submit">{saving ? 'Salvando…' : 'Salvar fabricante'}</button></div>
        </form>
      </Dialog>}
    </div>
  )
}
