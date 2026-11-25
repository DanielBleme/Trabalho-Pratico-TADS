import { useCallback, useState } from 'react'
import { CalendarDays, CarFront, CircleHelp, CreditCard, Factory, Gauge, Users } from 'lucide-react'
import Alugueis from './pages/Alugueis.jsx'
import Clientes from './pages/Clientes.jsx'
import Fabricantes from './pages/Fabricantes.jsx'
import Pagamentos from './pages/Pagamentos.jsx'
import Veiculos from './pages/Veiculos.jsx'
import { Toast } from './components/PageUi.jsx'

const sections = [
  { name: 'Clientes', icon: Users, component: Clientes },
  { name: 'Aluguéis', icon: CalendarDays, component: Alugueis },
  { name: 'Fabricantes', icon: Factory, component: Fabricantes },
  { name: 'Pagamentos', icon: CreditCard, component: Pagamentos },
  { name: 'Veículos', icon: CarFront, component: Veiculos },
]

function Sidebar({ active, onNavigate }) {
  return <aside className="sidebar">
    <a className="brand" href="#inicio" aria-label="LocaOps, início" onClick={() => onNavigate('Aluguéis')}>
      <span className="brand-mark"><Gauge size={23} strokeWidth={1.6} /></span><span className="brand-name">loca<span>ops</span></span>
    </a>
    <div className="sidebar-label">Sua locadora</div>
    <nav className="main-nav" aria-label="Navegação principal">
      {sections.map(({ name, icon: Icon }) => <button key={name} className={`nav-link ${active === name ? 'active' : ''}`} onClick={() => onNavigate(name)} type="button" aria-current={active === name ? 'page' : undefined}>
        <span className="nav-icon"><Icon size={16} strokeWidth={1.6} /></span><span>{name}</span>
      </button>)}
    </nav>
    <div className="sidebar-bottom"><div className="sidebar-mantra">Operação leve.<br />Caminhos livres.</div><div className="sidebar-version">LocaOps · Gestão de frota</div></div>
  </aside>
}

function Topbar({ onHelp }) {
  const currentDate = new Intl.DateTimeFormat('pt-BR', { day: 'numeric', month: 'long', year: 'numeric' }).format(new Date())
  return <header className="topbar">
    <div className="greeting"><strong>Gestão da locadora</strong><span>{currentDate}</span></div>
    <button className="help-button" type="button" onClick={onHelp}><CircleHelp size={15} /><span>Ajuda</span></button>
  </header>
}

export default function App() {
  const [active, setActive] = useState('Aluguéis')
  const [toast, setToast] = useState('')
  const notify = useCallback((message) => setToast(message), [])
  const Page = sections.find((section) => section.name === active)?.component || Alugueis

  return <div className="app-shell">
    <Sidebar active={active} onNavigate={setActive} />
    <main className="main-area">
      <Topbar onHelp={() => notify('Consulte a equipe da locadora se precisar de ajuda.')} />
      <div className="page-content"><Page notify={notify} /><footer className="page-footer"><span>LocaOps</span><span>Gestão simples para caminhos livres.</span></footer></div>
    </main>
    <Toast message={toast} onDismiss={() => setToast('')} />
  </div>
}
