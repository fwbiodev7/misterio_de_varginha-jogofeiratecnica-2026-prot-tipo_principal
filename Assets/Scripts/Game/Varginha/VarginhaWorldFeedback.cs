namespace Game.Varginha
{
    /// <summary>IMGUI world labels must not draw over modal Overlay canvases.</summary>
    public static class VarginhaWorldFeedback
    {
        public static bool IsHidden => VarginhaTravelCinematic.IsTravelling
            || VarginhaMainMenu.IsOpen
            || VarginhaGameHUD.Instance?.IsPaused == true
            || VarginhaGameHUD.Instance?.IsGameOver == true
            || VarginhaGameHUD.Instance?.IsInventoryOpen == true
            || VarginhaGameHUD.Instance?.IsDialogueOpen == true
            || VarginhaGameHUD.Instance?.IsVictoryOpen == true
            || Game.Managers.GameManager.Instance?.IsPaused == true;
    }
}
