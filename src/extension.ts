'use strict';
import { commands, ExtensionContext } from 'vscode'
import rewrapComments from './rewrapComments'


// Called when the extension is activated, the very first time the
// command is executed
export function activate(context: ExtensionContext) {

  context.subscriptions.push(
    commands.registerTextEditorCommand(
      'rewrap.rewrapComment', rewrapComments)
  )
}