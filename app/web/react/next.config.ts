import type { NextConfig } from 'next';
import createNextIntlPlugin from 'next-intl/plugin';

const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts');

const nextConfig: NextConfig = {
  // 关闭 React Strict Mode（开发环境不再双重渲染）
  reactStrictMode: true,
  // 生产容器化部署使用 standalone 模式
  output: 'standalone',
  // Server Actions 请求体大小限制（默认 1MB，增加到 5MB 支持文件上传）
  experimental: {
    serverActions: {
      bodySizeLimit: '5mb',
    },
  },
  // 允许加载外部图片域名
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'midasmarkets.s3.amazonaws.com',
        pathname: '/**',
      },
      {
        protocol: 'https',
        hostname: '*.amazonaws.com',
        pathname: '/**',
      },
    ],
  },
  // 将 /images/* 代理到 S3（开发和生产均需要，next/image 优化时会向自身发起此路径请求）
  async rewrites() {
    return [
      {
        source: '/images/:path*',
        destination: 'https://mm-front-public.s3.ap-southeast-2.amazonaws.com/images/:path*',
      },
    ];
  },
  webpack(config) {
    // 找到处理 SVG 的现有规则并排除 @/assets/icons 目录
    const fileLoaderRule = config.module.rules.find((rule: { test?: RegExp }) =>
      rule.test?.test?.('.svg')
    );

    // SVGR 处理的目录正则
    const svgrInclude = /src[\\/]assets[\\/]icons|public[\\/]images[\\/]icons/;

    config.module.rules.push(
      // 对指定目录中的 SVG 使用 SVGR (支持 currentColor)
      {
        test: /\.svg$/,
        include: svgrInclude,
        use: [
          {
            loader: '@svgr/webpack',
            options: {
              svgoConfig: {
                plugins: [
                  {
                    name: 'preset-default',
                    params: {
                      overrides: {
                        removeViewBox: false,
                      },
                    },
                  },
                  {
                    name: 'convertColors',
                    params: {
                      currentColor: true,
                    },
                  },
                ],
              },
            },
          },
        ],
      },
      // 其他 SVG 文件使用原来的 file-loader
      {
        ...fileLoaderRule,
        test: /\.svg$/,
        exclude: svgrInclude,
      }
    );

    // 从原规则中排除 SVG
    if (fileLoaderRule) {
      fileLoaderRule.exclude = /\.svg$/;
    }

    return config;
  },
};

export default withNextIntl(nextConfig);
