// Public surface of the Board module.
export { boardRoutes } from './board.routes';
export { BoardApi } from './infrastructure/board.api';
export type { BoardMember, TitleDef } from './core/title';
export { BoardActions } from './features/board-actions/board-actions';
export { TitleSelect } from './features/title-select/title-select';
