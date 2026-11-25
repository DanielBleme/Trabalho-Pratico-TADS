import React, { useCallback, useEffect, useState } from 'react'
import { Mail, Pencil, Phone, Plus, Search, Trash2, UserRound, Users } from 'lucide-react'
import { api, initials, query } from '../api.js'
import { DataFeedback, Dialog, FormError, PageCard, PageHeader } from '../components/PageUi.jsx'

const emptyClient = { nome: '', cpf: '', email: '', telefone: '' }

function maskCpf(value) {
  return value.replace(/\D/g, '').slice(0, 11)
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d)/, '.$1-$2')
}

export default function Clientes({ notify }) {
  const [busca, setBusca] = useState('')
  const [clientes, setClientes] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialog, setDialog] = useState(null)
  const [form, setForm] = useState(emptyClient)
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  const load = useCallback(async (signal) => {
    setLoading(true); setError('')
    try { setClientes(await api(`/clientes${query({ busca })}`, { signal })) }
    catch (err) { if (err.name !== 'AbortError') setError(err.message) }
    finally { setLoading(false) }
  }, [busca])

  useEffect(() => {
    const controller = new AbortController()
    load(controller.signal)
    return () => controller.abort()
  }, [load])

  function openCreate() { setForm(emptyClient); setFormError(''); setDialog({ mode: 'create' }) }
  function openEdit(item) { setForm({ nome: item.nome, cpf: item.cpf, email: item.email, telefone: item.telefone }); setFormError(''); setDialog({ mode: 'edit', item }) }

  async function save(event) {
    event.preventDefault()
    if (form.nome.trim().length < 2) return setFormError('Informe o nome completo do cliente.')
    if (form.cpf.replace(/\D/g, '').length !== 11) return setFormError('Informe um CPF com 11 números.')
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) return setFormError('Informe um e-mail válido.')
    if (form.telefone.replace(/\D/g, '').length < 10) return setFormError('Informe um telefone com DDD.')
    setSaving(true); setFormError('')
    try {
      const body = { Nome: form.nome.trim(), CPF: form.cpf, Email: form.email.trim(), Telefone: form.telefone.trim() }
      if (dialog.mode === 'edit') await api(`/clientes/${dialog.item.id}`, { method: 'PUT', body: { ...body, Id: dialog.item.id } })
      else await api('/clientes', { method: 'POST', body })
      setDialog(null); notify(dialog.mode === 'edit' ? 'Cliente atualizado.' : 'Cliente cadastrado.')
      await load()
    } catch (err) { setFormError(err.message) }
    finally { setSaving(false) }
  }

  async function remove(item) {
    if (!window.confirm(`Excluir o cadastro de ${item.nome}?`)) return
    try { await api(`/clientes/${item.id}`, { method: 'DELETE' }); notify('Cliente removido.'); await load() }
    catch (err) { setError(err.message) }
  }

  return (
    <div className="section-page">
      <PageHeader eyebrow="Relacionamento" title="Clientes" description="Mantenha os dados dos clientes organizados e fáceis de encontrar." action={<button className="page-new-rental" type="button" onClick={openCreate}><Plus size={16} /> Novo cliente</button>} />
      <div className="summary-strip"><div><span className="summary-icon"><Users size={16} /></span><span><small>Clientes encontrados</small><strong>{clientes.length}</strong></span></div><div><span className="summary-icon"><UserRound size={16} /></span><span><small>Cadastros com busca atual</small><strong>{busca ? 'Filtrado' : 'Todos'}</strong></span></div></div>
      <PageCard>
        <div className="table-heading management-heading"><div><span className="section-kicker">Base de clientes</span><h3>Todos os clientes</h3></div><label className="table-search"><Search size={15} /><input value={busca} onChange={(event) => setBusca(event.target.value)} placeholder="Buscar por nome, CPF ou e-mail" aria-label="Buscar clientes por nome, CPF ou e-mail" /></label></div>
        <DataFeedback loading={loading} error={error} empty={clientes.length === 0 ? 'Nenhum cliente encontrado para essa busca.' : ''}>
          <div className="table-scroll"><table className="management-table"><thead><tr><th>Cliente</th><th>CPF</th><th>E-mail</th><th>Telefone</th><th className="actions-col">Ações</th></tr></thead><tbody>
            {clientes.map((item) => <tr key={item.id}><td><div className="entity-cell"><span className="customer-avatar avatar-sand">{initials(item.nome)}</span><strong>{item.nome}</strong></div></td><td>{item.cpf}</td><td><span className="table-contact"><Mail size={13} />{item.email}</span></td><td><span className="table-contact"><Phone size={13} />{item.telefone}</span></td><td className="actions-col"><button className="icon-action" title="Editar" onClick={() => openEdit(item)}><Pencil size={15} /></button><button className="icon-action danger-action" title="Excluir" onClick={() => remove(item)}><Trash2 size={15} /></button></td></tr>)}
          </tbody></table></div><div className="table-footer"><span>{clientes.length} clientes encontrados</span></div>
        </DataFeedback>
      </PageCard>

      {dialog && <Dialog eyebrow={dialog.mode === 'edit' ? 'Editar cadastro' : 'Novo cadastro'} title={dialog.mode === 'edit' ? 'Editar cliente' : 'Cadastrar cliente'} description="Os campos marcados são necessários para salvar o cadastro." onClose={() => setDialog(null)}>
        <form className="entity-form" onSubmit={save} noValidate>
          <label>Nome completo<input autoFocus maxLength="150" value={form.nome} onChange={(event) => setForm({ ...form, nome: event.target.value })} placeholder="Nome e sobrenome" /></label>
          <div className="form-grid-two"><label>CPF<input inputMode="numeric" value={form.cpf} onChange={(event) => setForm({ ...form, cpf: maskCpf(event.target.value) })} placeholder="000.000.000-00" /></label><label>Telefone<input value={form.telefone} maxLength="20" onChange={(event) => setForm({ ...form, telefone: event.target.value })} placeholder="(11) 90000-0000" /></label></div>
          <label>E-mail<input type="email" maxLength="150" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} placeholder="cliente@email.com" /></label>
          <FormError>{formError}</FormError>
          <div className="modal-actions"><button className="cancel-button" type="button" onClick={() => setDialog(null)}>Cancelar</button><button className="submit-button" disabled={saving} type="submit">{saving ? 'Salvando…' : 'Salvar cliente'}</button></div>
        </form>
      </Dialog>}
    </div>
  )
}
