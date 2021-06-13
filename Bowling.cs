using System;

public class BowlingGame
{
    private int finalScore = 0;
    private int frameScore = 0;
    private int frameCount = 0;
    private int rollCountInFrame = 0;
    private int[] FramePointMultiplier = new int[11] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    private int[] RollPointMultiplier = new int[21] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    private int rollCounter = 0;

    private Frame[] framesInGame = new Frame[11];
    int bonusFrame = 10;

    bool isSpare = false;

    int new_frameArrayIndex = 0;
    Frame currentFrame;
    int bonusScoreAfterLastStrike = 0;

    public BowlingGame()
    {
        for (int i = 0; i < 11; i++)
        {
            framesInGame[i] = new Frame();
        }
    }

    public void Roll(int pins)
    {
        if (pins < 0 || pins > 10 | new_frameArrayIndex > 10)
        {
            throw new ArgumentException();
        }

        if (!IsLastFrameASpare() && !IsLastFrameAStrike() && new_frameArrayIndex == 10)
        {
            throw new ArgumentException();
        }

        if (new_frameArrayIndex == bonusFrame)
        {
            var lastFrame = framesInGame[bonusFrame];

            if (IsLastFrameASpare())
            {
                lastFrame.DoRoll(pins);
                lastFrame.IsFrameCompleted = true;
                new_frameArrayIndex++;
                finalScore += pins;
            }
            else if (IsLastFrameAStrike())
            {
                if (lastFrame.IsPlayed() && !lastFrame.IsFrameCompleted && lastFrame.GetFrameScore() < 10 && (pins == 10 || lastFrame.GetFrameScore() + pins > 10))
                {
                    throw new ArgumentException();
                }
                lastFrame.DoRoll(pins);
                finalScore += pins;
                if (lastFrame.IsFrameCompleted)
                {
                    new_frameArrayIndex++;
                }
            }
        }
        else
        {
            currentFrame = framesInGame[new_frameArrayIndex];
            currentFrame.DoRoll(pins);
            if (currentFrame.IsStrike())
                currentFrame.IsFrameCompleted = true;

            if (currentFrame.GetFrameScore() > 10)
            {
                throw new ArgumentException();
            }

            if (currentFrame.IsStrike() && new_frameArrayIndex < 9)
            {
                GetNextFrame()?.ApplyStrike();
                if (IsPreviousFrameStrike())
                {
                    GetNextFrame()?.CarryPreviousStrikeBonus();
                }
            }
            else if (currentFrame.IsSpare())
            {
                GetNextFrame()?.ApplySpare();
            }

            if (currentFrame.IsFrameCompleted)
            {
                new_frameArrayIndex++;
                finalScore += currentFrame.GetFrameScoreWithBonus();
            }
        }

    }

    private Frame GetNextFrame()
    {
        return new_frameArrayIndex < 9 ? framesInGame[new_frameArrayIndex + 1] : null;
    }
    private Frame GetPreviousFrame()
    {
        return new_frameArrayIndex > 0 ? framesInGame[new_frameArrayIndex - 1] : null;
    }

    private bool IsPreviousFrameStrike()
    {
        return new_frameArrayIndex > 0 && framesInGame[new_frameArrayIndex - 1].IsStrike();
    }

    private bool IsPreviousFrameSpare()
    {
        return new_frameArrayIndex > 0 && framesInGame[new_frameArrayIndex - 1].IsSpare();
    }

    private bool IsLastFrameASpare()
    {
        return new_frameArrayIndex == 10 && IsPreviousFrameSpare();
    }

    private bool IsLastFrameAStrike()
    {
        return new_frameArrayIndex == 10 && IsPreviousFrameStrike();
    }

    public int? Score()
    {
        if (!framesInGame[0].IsPlayed()
            || new_frameArrayIndex < 9
            || (new_frameArrayIndex == 10 && (IsLastFrameAStrike() || IsLastFrameASpare()) && !framesInGame[new_frameArrayIndex].IsFrameCompleted))
            throw new ArgumentException();

        return finalScore;
    }
}

class Roll
{
    private int rollScore;
    private int bonusTimes;
    public bool IsPlayed { get; private set; }

    public void Play(int score)
    {
        IsPlayed = true;
        rollScore += score;
    }

    public void AddBonus()
    {
        bonusTimes += 1;
    }

    public int GetRollScore()
    {
        return rollScore;
    }

    public int GetRollScoreWithBonus()
    {
        return rollScore + (rollScore * bonusTimes);
    }
}

class Frame
{
    private int frameScore;
    private Roll firstRoll;
    private Roll secondRoll;

    public Frame()
    {
        firstRoll = new Roll();
        secondRoll = new Roll();
    }

    public void UpdateScoreTo(int score)
    {
        frameScore += score;
    }

    public bool IsSpare()
    {
        return firstRoll.IsPlayed && secondRoll.IsPlayed && firstRoll.GetRollScore() + secondRoll.GetRollScore() == 10;
    }

    public bool IsStrike()
    {
        return firstRoll.IsPlayed && !secondRoll.IsPlayed && firstRoll.GetRollScore() == 10;
    }

    public bool IsFrameCompleted { get; set; }

    public int GetFrameScoreWithBonus()
    {
        return firstRoll.GetRollScoreWithBonus() + secondRoll.GetRollScoreWithBonus();
    }

    public int GetFrameScore()
    {
        return firstRoll.GetRollScore() + secondRoll.GetRollScore();
    }

    public void DoRoll(int score)
    {
        if (!firstRoll.IsPlayed)
        {
            firstRoll.Play(score);
        }
        else
        {
            secondRoll.Play(score);
            IsFrameCompleted = true;
        }
    }

    public void ApplyStrike()
    {
        firstRoll.AddBonus();
        secondRoll.AddBonus();
    }

    public void CarryPreviousStrikeBonus()
    {
        firstRoll.AddBonus();
    }

    public void ApplySpare()
    {
        firstRoll.AddBonus();
    }

    public bool IsPlayed()
    {
        return firstRoll.IsPlayed;
    }
}