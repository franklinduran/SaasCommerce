import { createReadStream, existsSync, statSync } from 'node:fs'
import { createServer } from 'node:http'
import { extname, join, normalize, resolve } from 'node:path'

const host = process.env.HOST ?? '127.0.0.1'
const port = Number.parseInt(process.env.PORT ?? '5173', 10)
const root = resolve('dist')

const contentTypes = new Map([
  ['.css', 'text/css; charset=utf-8'],
  ['.html', 'text/html; charset=utf-8'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.png', 'image/png'],
  ['.svg', 'image/svg+xml'],
])

const server = createServer((request, response) => {
  const url = new URL(request.url ?? '/', `http://${host}:${port}`)
  const requestedPath = normalize(decodeURIComponent(url.pathname)).replace(/^[/\\]+/, '')
  const candidatePath = join(root, requestedPath || 'index.html')
  const filePath = getFilePath(candidatePath)

  response.setHeader(
    'Content-Type',
    contentTypes.get(extname(filePath)) ?? 'application/octet-stream',
  )

  createReadStream(filePath).pipe(response)
})

server.listen(port, host, () => {
  console.log(`SaasCommerce web available at http://${host}:${port}`)
})

function getFilePath(candidatePath) {
  if (
    candidatePath.startsWith(root) &&
    existsSync(candidatePath) &&
    statSync(candidatePath).isFile()
  ) {
    return candidatePath
  }

  return join(root, 'index.html')
}
