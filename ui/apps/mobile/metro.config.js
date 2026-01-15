// Learn more https://docs.expo.io/guides/customizing-metro
const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");
const path = require("path");

// Find the workspace root, this can be replaced with `find-yarn-workspace-root`
const workspaceRoot = path.resolve(__dirname, "../..");
const projectRoot = __dirname;

const config = getDefaultConfig(projectRoot);

// 1. Watch all files within the monorepo
config.watchFolders = [workspaceRoot];
// 2. Let Metro know where to resolve packages, and in what order
config.resolver.nodeModulesPaths = [
  path.resolve(projectRoot, "node_modules"),
  path.resolve(workspaceRoot, "node_modules"),
];
// 3. Enable pnpm compatibility by allowing hierarchical lookup
// This allows Metro to follow pnpm's symlink structure
config.resolver.disableHierarchicalLookup = false;
// 4. Enable package exports for sub-packages like jsx-runtime
config.resolver.unstable_enablePackageExports = true;

// 5. Custom resolver for react-native-css-interop subpaths
const defaultResolver = config.resolver.resolveRequest;
config.resolver.resolveRequest = (context, realModuleName, platform, moduleName) => {
  // Handle react-native-css-interop subpaths
  if (realModuleName?.startsWith("react-native-css-interop/")) {
    try {
      const resolvedPath = require.resolve(realModuleName, {
        paths: [
          path.resolve(projectRoot, "node_modules"),
          path.resolve(workspaceRoot, "node_modules"),
        ],
      });
      return {
        type: "sourceFile",
        filePath: resolvedPath,
      };
    } catch (e) {
      // Fall through to default resolver
    }
  }
  // Use default resolver for everything else
  if (defaultResolver) {
    return defaultResolver(context, realModuleName, platform, moduleName);
  }
  return context.resolveRequest(context, realModuleName, platform);
};

module.exports = withNativeWind(config, { input: "./global.css" });
