const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');

function createCopy(pathW = '') {
    return ({
        entry: './src/app.js',
        output: {
            path: path.resolve(__dirname, 'docs/' + pathW),
            filename: 'appBundle.js',
            publicPath: "/"
        },
        devServer: {
            historyApiFallback: {
                index: 'index.html',
                rewrites: [
                    // from any url that doesn't match the above, redirect to /index.html
                    { from: /./, to: '/index.html' },
                ]
            }
        },
        module: {
            rules: [
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    loader: 'babel-loader',
                },
                {
                    test: /\.(ts|tsx)$/,
                    exclude: /node_modules/,
                    loader: 'ts-loader',
                },
                {
                    test: /\.(css)$/,
                    use: [
                        'style-loader',
                        'css-loader',
                    ]
                },
                {
                    test: /\.(jpg|png|svg|ico|icns|glb)$/,
                    loader: 'file-loader',
                    options: {
                        name: '[path][name].[ext]',
                    },
                }
            ],
        },
        plugins: [
            new HtmlWebpackPlugin({
                filename: 'index.html',
                template: path.resolve(__dirname, './public/index.html'),
            })
        ]
    })
}

module.exports = [
    createCopy()
];
