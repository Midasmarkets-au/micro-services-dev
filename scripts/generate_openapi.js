#!/usr/bin/env node
/**
 * Parse proto/http/v1/*.proto files and generate OpenAPI 3.0 YAML
 * Uses regex-based parsing to extract services, RPCs, messages, and google.api.http annotations
 */

const fs = require('fs');
const path = require('path');
const yaml = require('js-yaml');

const PROTO_DIR = '/Users/lucaslee/code/edge-ark/Micro-Service/proto/http/v1';
const OUT_FILE = '/Users/lucaslee/code/edge-ark/Micro-Service/docs/openapi/openapi.yaml';

// ── Parser ────────────────────────────────────────────────────────────────────

function parseProtoFile(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const services = [];
  const messages = {};

  // Remove block comments
  const cleaned = content.replace(/\/\*[\s\S]*?\*\//g, '');

  // Parse messages
  const messageRegex = /^message\s+(\w+)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}/gm;
  let m;
  while ((m = messageRegex.exec(cleaned)) !== null) {
    const msgName = m[1];
    const body = m[2];
    const fields = [];
    const fieldRegex = /(?:optional\s+)?(\w+)\s+(\w+)\s*=\s*(\d+)\s*;/g;
    let f;
    while ((f = fieldRegex.exec(body)) !== null) {
      const isRepeated = body.slice(0, f.index).match(/repeated\s*$/);
      fields.push({
        type: f[1],
        name: f[2],
        number: parseInt(f[3]),
        repeated: !!isRepeated,
        optional: /optional\s+\w+\s+\w+\s*=\s*\d+/.test(body.slice(Math.max(0, f.index - 20), f.index + f[0].length)),
      });
    }
    // Also capture repeated fields
    const repeatedRegex = /repeated\s+(\w+)\s+(\w+)\s*=\s*(\d+)\s*;/g;
    let r;
    while ((r = repeatedRegex.exec(body)) !== null) {
      const existing = fields.find(f => f.name === r[2]);
      if (existing) {
        existing.repeated = true;
      } else {
        fields.push({ type: r[1], name: r[2], number: parseInt(r[3]), repeated: true, optional: false });
      }
    }
    messages[msgName] = fields;
  }

  // Parse services and RPCs
  const serviceRegex = /service\s+(\w+)\s*\{([\s\S]*?)^}/gm;
  while ((m = serviceRegex.exec(cleaned)) !== null) {
    const serviceName = m[1];
    const serviceBody = m[2];
    const rpcs = [];

    const rpcRegex = /\/\/\s*(.*?)\n\s*rpc\s+(\w+)\s*\((\w+)\)\s+returns\s+\((\w+)\)\s*\{([^}]*)\}/g;
    let r;
    while ((r = rpcRegex.exec(serviceBody)) !== null) {
      const comment = r[1].trim();
      const rpcName = r[2];
      const inputType = r[3];
      const outputType = r[4];
      const options = r[5];

      // Extract HTTP method and path
      const httpMatch = options.match(/(?:get|post|put|delete|patch):\s*"([^"]+)"/);
      const methodMatch = options.match(/(get|post|put|delete|patch):\s*"/);
      const bodyMatch = options.match(/body:\s*"([^"]*)"/);

      if (httpMatch && methodMatch) {
        rpcs.push({
          comment,
          name: rpcName,
          inputType,
          outputType,
          method: methodMatch[1],
          path: httpMatch[1],
          body: bodyMatch ? bodyMatch[1] : null,
        });
      }
    }

    // Also catch RPCs without leading comment
    const rpcNoCommentRegex = /rpc\s+(\w+)\s*\((\w+)\)\s+returns\s+\((\w+)\)\s*\{([^}]*)\}/g;
    while ((r = rpcNoCommentRegex.exec(serviceBody)) !== null) {
      const rpcName = r[1];
      if (rpcs.find(x => x.name === rpcName)) continue;
      const inputType = r[2];
      const outputType = r[3];
      const options = r[4];
      const httpMatch = options.match(/(?:get|post|put|delete|patch):\s*"([^"]+)"/);
      const methodMatch = options.match(/(get|post|put|delete|patch):\s*"/);
      const bodyMatch = options.match(/body:\s*"([^"]*)"/);
      if (httpMatch && methodMatch) {
        rpcs.push({
          comment: rpcName,
          name: rpcName,
          inputType,
          outputType,
          method: methodMatch[1],
          path: httpMatch[1],
          body: bodyMatch ? bodyMatch[1] : null,
        });
      }
    }

    if (rpcs.length > 0) {
      services.push({ name: serviceName, rpcs });
    }
  }

  return { services, messages };
}

// ── Type mapping ──────────────────────────────────────────────────────────────

function protoTypeToOpenApi(type, repeated) {
  const typeMap = {
    string: { type: 'string' },
    bool: { type: 'boolean' },
    int32: { type: 'integer', format: 'int32' },
    int64: { type: 'string', format: 'int64', description: '64-bit integer (string for JSON safety)' },
    uint32: { type: 'integer', format: 'int32' },
    uint64: { type: 'string', format: 'int64' },
    float: { type: 'number', format: 'float' },
    double: { type: 'number', format: 'double' },
    bytes: { type: 'string', format: 'byte' },
  };
  const base = typeMap[type] || { $ref: `#/components/schemas/${type}` };
  if (repeated) return { type: 'array', items: base };
  return base;
}

// ── OpenAPI builder ───────────────────────────────────────────────────────────

function buildOpenApi(allData) {
  const { allServices, allMessages } = allData;

  const openapi = {
    openapi: '3.0.3',
    info: {
      title: 'Bacera Gateway API',
      description: 'Multi-tenant trading/finance platform API — generated from proto/http/v1',
      version: '1.0.0',
    },
    servers: [{ url: 'http://localhost:9005', description: 'Local development' }],
    tags: [],
    paths: {},
    components: { schemas: {} },
  };

  // Build schemas from messages
  for (const [msgName, fields] of Object.entries(allMessages)) {
    const properties = {};
    const required = [];

    for (const field of fields) {
      if (['PaginationRequest', 'PaginationMeta'].includes(field.type)) {
        properties[field.name] = { $ref: `#/components/schemas/${field.type}` };
        continue;
      }
      const schema = protoTypeToOpenApi(field.type, field.repeated);
      properties[field.name] = schema;
      if (!field.optional && !field.repeated && field.name !== 'pagination') {
        // Don't mark as required for now to keep it permissive
      }
    }

    openapi.components.schemas[msgName] = {
      type: 'object',
      properties,
    };
  }

  // Build paths from services
  for (const { name: serviceName, rpcs } of allServices) {
    // Derive tag from service name
    const tag = serviceName
      .replace(/Service(V\d+)?$/, '')
      .replace(/([A-Z])/g, ' $1')
      .trim();

    if (!openapi.tags.find(t => t.name === tag)) {
      openapi.tags.push({ name: tag });
    }

    for (const rpc of rpcs) {
      const { method, path: rpcPath, inputType, outputType, comment, body } = rpc;

      // Extract path parameters
      const pathParams = [];
      const pathParamRegex = /\{(\w+)\}/g;
      let pp;
      while ((pp = pathParamRegex.exec(rpcPath)) !== null) {
        pathParams.push(pp[1]);
      }

      // Convert path params from snake_case to {param}
      const openApiPath = rpcPath; // already in {param} format

      if (!openapi.paths[openApiPath]) {
        openapi.paths[openApiPath] = {};
      }

      const inputFields = allMessages[inputType] || [];
      const parameters = [];

      // Path parameters
      for (const paramName of pathParams) {
        const field = inputFields.find(f => f.name === paramName);
        parameters.push({
          name: paramName,
          in: 'path',
          required: true,
          schema: field ? protoTypeToOpenApi(field.type, false) : { type: 'string' },
        });
      }

      // Query parameters (fields not in path and not body)
      if (['get', 'delete'].includes(method) || !body) {
        for (const field of inputFields) {
          if (pathParams.includes(field.name)) continue;
          if (field.name === 'pagination') {
            // Expand pagination fields
            parameters.push(
              { name: 'pagination.page', in: 'query', schema: { type: 'integer', default: 1 } },
              { name: 'pagination.size', in: 'query', schema: { type: 'integer', default: 20 } },
              { name: 'pagination.sort_field', in: 'query', schema: { type: 'string' } },
              { name: 'pagination.sort_desc', in: 'query', schema: { type: 'boolean' } },
            );
            continue;
          }
          parameters.push({
            name: field.name,
            in: 'query',
            required: false,
            schema: protoTypeToOpenApi(field.type, field.repeated),
          });
        }
      }

      const operation = {
        tags: [tag],
        summary: comment || rpc.name,
        operationId: `${serviceName}_${rpc.name}`,
        parameters: parameters.length > 0 ? parameters : undefined,
        responses: {
          '200': {
            description: 'Success',
            content: {
              'application/json': {
                schema: allMessages[outputType]
                  ? { $ref: `#/components/schemas/${outputType}` }
                  : { type: 'object' },
              },
            },
          },
          '400': { description: 'Bad Request' },
          '401': { description: 'Unauthorized' },
          '403': { description: 'Forbidden' },
          '500': { description: 'Internal Server Error' },
        },
      };

      // Request body for POST/PUT
      if (['post', 'put', 'patch'].includes(method) && body !== undefined) {
        const bodyFields = inputFields.filter(f => !pathParams.includes(f.name));
        if (bodyFields.length > 0) {
          const bodySchemaName = body && body !== '*' ? inputType + '_Body' : inputType;
          if (body && body !== '*') {
            // Body is a specific field
            const bodyField = inputFields.find(f => f.name === body);
            if (bodyField) {
              operation.requestBody = {
                content: {
                  'application/json': {
                    schema: { $ref: `#/components/schemas/${bodyField.type}` },
                  },
                },
              };
            }
          } else {
            operation.requestBody = {
              content: {
                'application/json': {
                  schema: { $ref: `#/components/schemas/${inputType}` },
                },
              },
            };
          }
        }
      }

      openapi.paths[openApiPath][method] = operation;
    }
  }

  return openapi;
}

// ── Main ──────────────────────────────────────────────────────────────────────

const protoFiles = fs.readdirSync(PROTO_DIR)
  .filter(f => f.endsWith('.proto') && f !== 'common.proto')
  .map(f => path.join(PROTO_DIR, f));

// Always include common.proto first
const commonPath = path.join(PROTO_DIR, 'common.proto');
const allData = { allServices: [], allMessages: {} };

// Parse common first
const common = parseProtoFile(commonPath);
Object.assign(allData.allMessages, common.messages);

// Parse all others
for (const file of protoFiles) {
  const { services, messages } = parseProtoFile(file);
  Object.assign(allData.allMessages, messages);
  allData.allServices.push(...services);
}

const openapi = buildOpenApi(allData);

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, yaml.dump(openapi, { lineWidth: 120, noRefs: true }));

console.log(`Generated: ${OUT_FILE}`);
console.log(`  Services: ${allData.allServices.length}`);
console.log(`  Paths: ${Object.keys(openapi.paths).length}`);
console.log(`  Schemas: ${Object.keys(openapi.components.schemas).length}`);
