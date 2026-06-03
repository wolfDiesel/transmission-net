import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppToastHost } from './components/AppToast'
import { AppProvider } from './context/AppProvider'
import { TorrentListProvider } from './context/TorrentListProvider'
import { AppShell } from './layout/AppShell'
import { TorrentAssociationPrompt } from './components/desktop/TorrentAssociationPrompt'
import { TorrentLaunchRedirect } from './components/desktop/TorrentLaunchRedirect'
import { TorrentLaunchWatcher } from './components/desktop/TorrentLaunchWatcher'
import { ThemeSync } from './theme/ThemeSync'
import { AddTorrentPage } from './pages/AddTorrentPage'
import { SettingsPage } from './pages/SettingsPage'
import { TorrentsPage } from './pages/TorrentsPage'

export function App() {
  return (
    <BrowserRouter>
      <AppProvider>
        <TorrentListProvider>
          <ThemeSync />
          <TorrentLaunchRedirect />
          <TorrentLaunchWatcher />
          <TorrentAssociationPrompt />
          <AppToastHost />
          <Routes>
            <Route element={<AppShell />}>
              <Route index element={<TorrentsPage />} />
              <Route path="add" element={<AddTorrentPage />} />
              <Route path="settings" element={<SettingsPage />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
          </Routes>
        </TorrentListProvider>
      </AppProvider>
    </BrowserRouter>
  )
}
