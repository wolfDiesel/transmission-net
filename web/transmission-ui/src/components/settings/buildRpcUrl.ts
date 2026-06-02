import type { DaemonConnectionDto } from '../../api/types'

export function buildRpcUrl(daemon: Pick<DaemonConnectionDto, 'host' | 'port' | 'rpcPath'>): string {
  const path = daemon.rpcPath.startsWith('/') ? daemon.rpcPath : `/${daemon.rpcPath}`
  return `http://${daemon.host}:${daemon.port}${path}`
}
