using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private List<RankingTab> _rankingTabs;
    [SerializeField] private Sprite[] medarSprites;

    private List<RankingTab> _showedRankingTab;

    private void Awake()
    {
        _showedRankingTab = new List<RankingTab>(_rankingTabs.Count);
        for (int i = 0; i < _rankingTabs.Count; i++)
        {
            _rankingTabs[i].Deactive();
        }

    }

    public void ActiveUser(int id, string userName, Color32 playerColor)
    {
        _rankingTabs[id].ActiveUser(id, userName, playerColor);
        _showedRankingTab.Add(_rankingTabs[id]);

        ReSortList();
    }

    public bool IsUserActived(int id)
    {
        return _rankingTabs[id].IsActive;
    }

    public void DeactiveUser(int id)
    {
        for (int i = 0; i < _showedRankingTab.Count; i++)
        {
            if (_showedRankingTab[i].PlayerID == id)
            {
                _showedRankingTab[i].Deactive();
                _showedRankingTab.RemoveAt(i);
                break;
            }
        }
    }

    public void UpdateScore(int playerID, int newScore)
    {
        for (int i = 0; i < _showedRankingTab.Count; i++)
        {
            if (_showedRankingTab[i].PlayerID == playerID)
            {
                _showedRankingTab[i].SetScore(newScore);
            }
        }
        ReSortList();
    }

    private void ReSortList()
    {
        _showedRankingTab.Sort(new SortRankingTab());

        for (int i = 0; i < _showedRankingTab.Count; i++)
        {
            if (_showedRankingTab[i].CurScore <= 0)
            {
                _showedRankingTab[i].transform.SetAsLastSibling();
                continue;
            }

            if (i <= 2)
            {
                _showedRankingTab[i].SetRank(medarSprites[i]);
                _showedRankingTab[i].transform.SetAsLastSibling();
            }
            else
            {
                _showedRankingTab[i].SetRank(i + 1);
                _showedRankingTab[i].transform.SetAsLastSibling();
            }
  
        }
    }
}

public class SortRankingTab: IComparer<RankingTab>
{
    public int Compare(RankingTab x, RankingTab y)
    {
        if (x.CurScore >= y.CurScore)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}
